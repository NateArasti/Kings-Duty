using Godot;

public partial class CharacterFollower : CharacterBody3D
{
	[Export] public Node3D FollowTarget { get; set; }
	[Export] public Vector3 FollowOffset { get; set; }
	[Export] public bool StrictOffsetFollow { get; set; } = true;
	
	[Export] private int m_ObstacleAvoidanceRaysCount = 8;
	[Export] private float m_ObstacleAvoidanceMinLength = 1;
	[Export] private float m_ObstacleAvoidanceMaxLength = 1.5f;
	
	private RayCast3D[] m_Rays;
	
	private Vector3 m_TargetPreviousPosition;
	private Vector3 m_CurrentTargetDirection;
	
	private bool m_IsTargetMoving;
	
	protected readonly FollowSettings SimpleFollowSettings = new()
	{
		FollowSpaceDistance = 0.5f,
		MoveSpeedGradient = new Vector2(1.3f, 3),
		RotationSpeed = 2,
	};
	
	protected bool LaggingTarget { get; private set; }

	public FollowSettings Settings { get; set; }
	
	public override void _Ready()
	{
		base._Ready();
		
		Settings = SimpleFollowSettings;
		
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
		base._Process(delta);
		if (FollowTarget == null) return;
		
		m_IsTargetMoving = m_TargetPreviousPosition != FollowTarget.GlobalPosition;
		if (m_IsTargetMoving)
		{
			m_CurrentTargetDirection = FollowTarget.GlobalPosition - m_TargetPreviousPosition;
			m_CurrentTargetDirection.Y = 0;
		}
		m_TargetPreviousPosition = FollowTarget.GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (FollowTarget == null) return;
		
		var desiredMoveVector = FollowTarget.GlobalPosition + GetDynamicOffset() - GlobalPosition;
		desiredMoveVector.Y = 0;
		
		var distanceToTarget = desiredMoveVector.Length();
		LaggingTarget = distanceToTarget > Settings.FollowSpaceDistance;
		if (!LaggingTarget)
		{
			Velocity = Velocity.Lerp(Vector3.Zero, 2 * Settings.RotationSpeed * (float) delta);
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
			var dynamicMoveSpeed = Settings.MoveSpeedGradient.X + moveSpeedGrade * (Settings.MoveSpeedGradient.Y - Settings.MoveSpeedGradient.X);
			Velocity = Velocity.Lerp(mostAwailableMoveVector.MoveVector.Normalized() * dynamicMoveSpeed, Settings.RotationSpeed * (float)delta);
		}
		
		MoveAndSlide();
	}
	
	private Vector3 GetDynamicOffset()
	{
		if (StrictOffsetFollow)
		{
			var angle = Vector3.Right.SignedAngleTo(m_CurrentTargetDirection, Vector3.Up);
			return FollowOffset.Rotated(Vector3.Up, angle);			
		}
		
		return FollowOffset;
	}
	
	public struct FollowSettings
	{
		public float FollowSpaceDistance = 0.25f;
		public Vector2 MoveSpeedGradient = new Vector2(1, 4);
		public float RotationSpeed = 1;

		public FollowSettings() { }
	}
}
