using Godot;

public partial class CharacterFollower : CharacterBody3D
{
	[Export] public Node3D Target { get; set; }
	[Export] public Vector3 FollowOffset { get; set; }
	
	[Export] private float m_FollowSpaceDistance = 0.25f;
	[Export(PropertyHint.Range, "0.1,10")] private Vector2 m_MoveSpeedGradient = new Vector2(1, 4);
	[Export] private float m_RotationSpeed = 1;
	[Export] private int m_ObstacleAvoidanceRaysCount = 8;
	[Export] private float m_ObstacleAvoidanceMinLength = 1;
	[Export] private float m_ObstacleAvoidanceMaxLength = 1.5f;
	
	private RayCast3D[] m_Rays;
	
	private Vector3 m_TargetPreviousPosition;
	private Vector3 m_CurrentTargetDirection;
	
	private bool m_IsTargetMoving;

	public override void _Ready()
	{
		m_Rays = new RayCast3D[m_ObstacleAvoidanceRaysCount];
		for (var i = 0; i < m_ObstacleAvoidanceRaysCount; ++i)
		{
			var ray = new RayCast3D();
			var direction = Vector2.Right;
			direction = direction.Rotated(2 * Mathf.Pi * i / m_ObstacleAvoidanceRaysCount);
			ray.TargetPosition = new Vector3(direction.X, 0, direction.Y).Normalized() * m_ObstacleAvoidanceMinLength;
			m_Rays[i] = ray;
			AddChild(ray);
		}
	}

	public override void _Process(double delta)
	{
		if (Target == null) return;
		
		m_IsTargetMoving = m_TargetPreviousPosition != Target.GlobalPosition;
		if (m_IsTargetMoving)
		{
			m_CurrentTargetDirection = Target.GlobalPosition - m_TargetPreviousPosition;
			m_CurrentTargetDirection.Y = 0;
		}
		m_TargetPreviousPosition = Target.GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		var desiredMoveVector = Target.GlobalPosition + GetDynamicOffset() - GlobalPosition;
		desiredMoveVector.Y = 0;
		
		var distanceToTarget = desiredMoveVector.Length();
		if(distanceToTarget < m_FollowSpaceDistance)
		{
			Velocity = Velocity.Lerp(Vector3.Zero, 2 * m_RotationSpeed * (float) delta);
		}
		else
		{
			(Vector3 MoveVector, float Weight) mostAwailableMoveVector = (Vector3.Zero, -100f);
			for (int i = 0; i < m_Rays.Length; i++)
			{
				var ray = m_Rays[i].TargetPosition;
				var weight = desiredMoveVector.Dot(ray);
				if (m_Rays[i].IsColliding())
				{
					var distance = GlobalPosition.DistanceTo(m_Rays[i].GetCollisionPoint());
					weight *= distance / m_ObstacleAvoidanceMaxLength;
					
					if (distance < m_ObstacleAvoidanceMinLength)
					{
						ray *= -1;
						weight = 0.75f;
					}
				}
				if(weight > mostAwailableMoveVector.Weight)
				{
					mostAwailableMoveVector = (ray, weight);
				}
			}
			var moveSpeedGrade = Mathf.Clamp(distanceToTarget / (2 * m_ObstacleAvoidanceMaxLength), 0, 1);
			var dynamicMoveSpeed = m_MoveSpeedGradient.X + moveSpeedGrade * (m_MoveSpeedGradient.Y - m_MoveSpeedGradient.X);
			Velocity = Velocity.Lerp(mostAwailableMoveVector.MoveVector.Normalized() * dynamicMoveSpeed, m_RotationSpeed * (float)delta);
		}
		
		MoveAndSlide();
	}
	
	private Vector3 GetDynamicOffset()
	{
		var angle = Vector3.Right.SignedAngleTo(m_CurrentTargetDirection, Vector3.Up);
		return FollowOffset.Rotated(Vector3.Up, angle);
	}
}
