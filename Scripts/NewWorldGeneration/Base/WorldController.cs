using Godot;

namespace WorldGeneration
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
		
		[Export] private ChunkGenerator m_ChunkGenerator;
		[Export] private float m_MinTimeToGenerateChunk = 2;
		
		private int m_CurrentChunkIndex;
		private float m_CurrentTimeInChunk = 0;
	
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

		private void RegenerateChunksAround(int m_CurrentChunkIndex)
		{
		}

		private int GetPlayerChunk()
		{
			var player = PlayerGlobalController.Instance.Player;
			var globalChunkIndex = m_ChunkGenerator.GetCorrespondingChunkIndex(player.GlobalPosition);
			var localChunkIndex = globalChunkIndex - m_CurrentChunks[0].Index;
			return Mathf.Clamp(localChunkIndex, 0, k_RuntimeChunksCount);
		}
	}
}
