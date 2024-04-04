using System.Collections.Generic;
using Godot;

namespace WorldGeneration.Tiled
{
	/// <summary>
	/// Controls world generation upon player position with surrounding chunks
	/// Chunks are set like that
	///
	/// 	 <6>
	///   <3>	<7>
	/// <0>  <4>   <8>
	///   <1>   <5>
	///      <2>
	///      
	/// Player is always at center(4) chunk
	/// </summary>
	public partial class WorldController : Node
	{
		public const int k_RuntimeChunksCount = 9;
		
		[Export] private TileChunkGenerator m_ChunkGenerator;
		[Export] private float m_MinTimeToGenerateChunk = 2;
	
		[ExportGroup("Grid")]
		[Export] private Vector2 m_StepValue = Vector2.One;
		[Export] private Vector2 m_XGridDirection = new Vector2(1, -1);
		[Export] private Vector2 m_YGridDirection = new Vector2(1, 1);
		
		private readonly List<IWorldGenerationSubscriber> m_WorldGenerationSubscribers = new();
		
		private int m_CurrentChunkIndex;
		private float m_CurrentTimeInChunk = 0;
		
		private Vector2[] m_ChunkOffsets;
		private Vector2 m_GlobalOffset;
		private Vector2 m_StartOffset;
	
		private readonly int[][] m_ChunkSubgroups = new int[k_RuntimeChunksCount][]
		{
			new[] { 0, 1, 3, 4 },
			new[] { 0, 1, 2, 3, 4, 5 },
			new[] { 1, 2, 4, 5 },
			new[] { 0, 1, 3, 4, 6, 7 },
			null,
			new[] { 1, 2, 4, 5, 7, 8 },
			new[] { 3, 4, 6, 7 },
			new[] { 3, 4, 5, 6, 7, 8 },
			new[] { 4, 5, 7, 8 },
		};
		
		private readonly ChunkInstance[] m_CurrentChunks = new ChunkInstance[k_RuntimeChunksCount];

		public override void _Ready()
		{
			base._Ready();
			
			var rectSize = 2 * m_ChunkGenerator.CellSize * (Vector2)m_ChunkGenerator.TilesCountPerChunk * m_StepValue;
			m_ChunkOffsets = new Vector2[k_RuntimeChunksCount]
			{
				new Vector2(-1.5f, 0) * rectSize,
				new Vector2(-1f, -0.5f) * rectSize,
				new Vector2(-0.5f, -1f) * rectSize,
				new Vector2(-1f, 0.5f) * rectSize,
				new Vector2(-0.5f, 0) * rectSize,
				new Vector2(0, -0.5f) * rectSize,
				new Vector2(-0.5f, 1) * rectSize,
				new Vector2(0, 0.5f) * rectSize,
				new Vector2(0.5f, 0) * rectSize,
			};
			
			m_StartOffset = m_ChunkOffsets[0] + Vector2.Right * m_StepValue * m_ChunkGenerator.CellSize;
			m_GlobalOffset = m_StartOffset;
			
			foreach (var child in GetChildren())
			{
				if (child is IWorldGenerationSubscriber worldGenerationSubscriber)
				{
					worldGenerationSubscriber.Init(k_RuntimeChunksCount, m_ChunkGenerator.TilesCountPerChunk, m_ChunkGenerator.CellSize, GetGridCoords, GetWorldCoords);
					m_WorldGenerationSubscribers.Add(worldGenerationSubscriber);
				}
			}
			
			for (var i = 0; i < k_RuntimeChunksCount; ++i)
			{
				if(m_CurrentChunks[i] != null) continue;
				GenerateChunk(i);
			}
			
			var convertedGlobalOffset = Convert2DTo3D(m_GlobalOffset);
			foreach (var subscriber in m_WorldGenerationSubscribers)
			{
				subscriber.UpdateAllChunks(m_CurrentChunks, convertedGlobalOffset);
			}
		}

		public override void _Process(double delta)
		{
			var chunkIndex = GetPlayerChunk();
			
			if(m_CurrentChunkIndex != chunkIndex)
			{
				m_CurrentTimeInChunk = 0;			
			}
			
			m_CurrentChunkIndex = chunkIndex;
			
			if(m_CurrentChunkIndex != 4)
			{
				m_CurrentTimeInChunk += (float)delta;
			}
			
			if(m_CurrentTimeInChunk > m_MinTimeToGenerateChunk)
			{
				m_CurrentTimeInChunk = 0;
				
				RegenerateChunksAround(m_CurrentChunkIndex);
			}
		}

		private void RegenerateChunksAround(int currentChunkIndex)
		{
		}
		
		private void GenerateChunk(int localChunkIndex)
		{
			m_CurrentChunks[localChunkIndex] = m_ChunkGenerator.GenerateChunk(localChunkIndex);
			
			var convertedGlobalOffset = Convert2DTo3D(m_GlobalOffset);
			foreach (var subscriber in m_WorldGenerationSubscribers)
			{
				subscriber.OnChunkGenerated(m_CurrentChunks[localChunkIndex], convertedGlobalOffset);
			}
		}

		private int GetPlayerChunk()
		{
			var player = PlayerGlobalController.Instance.Player;
			var globalChunkIndex = m_ChunkGenerator.GetCorrespondingChunkIndex(Convert3DTo2D(player.GlobalPosition));
			var localChunkIndex = globalChunkIndex - m_CurrentChunks[0].Index;
			return Mathf.Clamp(localChunkIndex, 0, k_RuntimeChunksCount);
		}
	
		private Vector3 GetWorldCoords(Vector2I gridCoords)
		{
			var coords = m_ChunkGenerator.CellSize * m_StepValue * (gridCoords.X * m_XGridDirection + gridCoords.Y * m_YGridDirection);
			return Convert2DTo3D(coords);
		}
		
		private Vector2I GetGridCoords(Vector3 worldCoords)
		{
			var c = Convert3DTo2D(worldCoords) / (m_ChunkGenerator.CellSize * m_StepValue);
			var (x, y) = MathExtensions.Solve(m_XGridDirection.X, m_YGridDirection.X, c.X, m_XGridDirection.Y, m_YGridDirection.Y, c.Y);
			return new Vector2I(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
		}
		
		private static Vector3 Convert2DTo3D(Vector2 original)
		{
			return new Vector3(original.X, 0, original.Y);
		}
		
		private static Vector2 Convert3DTo2D(Vector3 original)
		{
			return new Vector2(original.X, original.Z);
		}
	}
}
