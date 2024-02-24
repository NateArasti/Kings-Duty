using Godot;
using System.Collections.Generic;
using System.Linq;

public class WorldGenerator
{
	private const int k_BezierResolution = 500;
	
	public enum CellType
	{
		Empty,
		Ground,
		Road,
		PointOfInterest
	}
	
	public Vector2 ChunkSize { get; }
	public float CellSize { get; }
	
	public int Width { get; }
	public int Height { get; }
	public float MinDistance { get; }
	public float RemoveEdgeChance { get; }
	
	private readonly CellType[] m_ChunkArrayBuffer;
	private readonly List<Vector2I> m_EdgeCellsBuffer = new();
	private readonly List<Vector2I> m_RoadCellsBuffer = new();
	private readonly List<Vector2I> m_PointOfInterestCellsBuffer = new();
	private readonly List<Vector2I> m_GroundCellsBuffer = new();
	
	public WorldGenerator(Vector2I chunkSize, float cellSize, float minDIstance, float removeEdgeChance = 0.25f)
	{
		Width = chunkSize.X;
		Height = chunkSize.Y;
		CellSize = cellSize;
		MinDistance = minDIstance;
		RemoveEdgeChance = removeEdgeChance;
		
		m_ChunkArrayBuffer = new CellType[Width * Height];
		ChunkSize = new Vector2(Width, Height) * CellSize;
	}
	
	public Chunk GenerateChunk(
		IReadOnlyList<Vector2> leftBorderStartPositions = null,
		IReadOnlyList<Vector2> topBorderStartPositions = null,
		IReadOnlyList<Vector2> rightBorderStartPositions = null,
		IReadOnlyList<Vector2> bottomBorderStartPositions = null)
	{
		ClearChunkBuffers();

		var startPositions = GetStartPositions(leftBorderStartPositions, topBorderStartPositions, rightBorderStartPositions, bottomBorderStartPositions);
		var borderPointsCount = startPositions.Count;
		
		var rect = new Rect2(Vector2.Zero, ChunkSize);
		var step = rect.Size * 0.05f;
		rect.Size *= 0.9f;
		rect.Position += step;
		var sampledPositions = PoissonSampler.SamplePositions(rect, MinDistance, startPositions);
		var triangles = DelaunayTriangulator.Triangulate(sampledPositions);
		var edges = RemoveRandomEdges(triangles, borderPointsCount).ToList();
		
		foreach (var edge in edges)
		{
			foreach (var point in GetCurveEdgePoints(sampledPositions[edge.StartIndex], sampledPositions[edge.EndIndex]))
			{
				var chunkPosition = Utility.GetGridPosition(point, Vector2.Zero, CellSize);
				chunkPosition = chunkPosition.Clamp(Vector2I.Zero, new Vector2I(Width - 1, Height - 1));
				var index = Utility.GetFlatIndex(chunkPosition.X, chunkPosition.Y, Width);
				m_ChunkArrayBuffer[index] = CellType.Road;
				m_RoadCellsBuffer.Add(chunkPosition);
			}
		}
		for (var i = 0; i < sampledPositions.Count; ++i)
		{
			var chunkPosition = Utility.GetGridPosition(sampledPositions[i], Vector2.Zero, CellSize);
			chunkPosition = chunkPosition.Clamp(Vector2I.Zero, new Vector2I(Width - 1, Height - 1));
			var index = Utility.GetFlatIndex(chunkPosition.X, chunkPosition.Y, Width);
			m_ChunkArrayBuffer[index] = CellType.PointOfInterest;
			m_PointOfInterestCellsBuffer.Add(chunkPosition);
			if (i < borderPointsCount)
				m_EdgeCellsBuffer.Add(chunkPosition);
		}
		for (var i = 0; i < m_ChunkArrayBuffer.Length; ++i)
		{
			if(m_ChunkArrayBuffer[i] == CellType.Empty)
			{
				m_ChunkArrayBuffer[i] = CellType.Ground;
				var chunkPosition = Utility.Get2DIndex(i, Width);
				m_GroundCellsBuffer.Add(chunkPosition);
			}
		}
		
		return new(
			(CellType[])m_ChunkArrayBuffer.Clone(),
			m_EdgeCellsBuffer.ToArray(),
			m_PointOfInterestCellsBuffer.ToArray()
		);
	}

	private List<Vector2> GetStartPositions(IReadOnlyList<Vector2> leftBorderStartPositions, IReadOnlyList<Vector2> topBorderStartPositions, IReadOnlyList<Vector2> rightBorderStartPositions, IReadOnlyList<Vector2> bottomBorderStartPositions)
	{
		var startPositions = new List<Vector2>();
		if (leftBorderStartPositions != null)
		{
			startPositions.AddRange(leftBorderStartPositions);
		}
		else
		{
			startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Down * ChunkSize.Y, MinDistance));
		}
		if (topBorderStartPositions != null)
		{
			startPositions.AddRange(topBorderStartPositions);
		}
		else
		{
			startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Right * ChunkSize.X, MinDistance));
		}
		if (rightBorderStartPositions != null)
		{
			startPositions.AddRange(rightBorderStartPositions);
		}
		else
		{
			startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Right * ChunkSize.X, ChunkSize, MinDistance));
		}
		if (bottomBorderStartPositions != null)
		{
			startPositions.AddRange(bottomBorderStartPositions);
		}
		else
		{
			startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Down * ChunkSize.Y, ChunkSize, MinDistance));
		}

		return startPositions;
	}

	private void ClearChunkBuffers()
	{
		m_EdgeCellsBuffer.Clear();
		m_RoadCellsBuffer.Clear();
		m_GroundCellsBuffer.Clear();
		m_PointOfInterestCellsBuffer.Clear();
		for (var i = 0; i < m_ChunkArrayBuffer.Length; ++i)
		{
			m_ChunkArrayBuffer[i] = CellType.Empty;
		}
	}
	
	private float[] GetEdgesPresences(List<DelaunayTriangulator.Edge> edges, List<Vector2> positions, int borderPointsCount)
	{
		var graph = new bool[positions.Count][];
		var edgeWeights = new float[edges.Count];
		for (int i = 0; i < edges.Count; i++)
		{
			if (graph[edges[i].StartIndex] == null)
			{
				graph[edges[i].StartIndex] = new bool[positions.Count];
			}
			graph[edges[i].StartIndex][edges[i].EndIndex] = true;
			if (graph[edges[i].EndIndex] == null)
			{
				graph[edges[i].EndIndex] = new bool[positions.Count];
			}
			graph[edges[i].EndIndex][edges[i].StartIndex] = true;
			
			edgeWeights[i] = 0;
		}
		
		var minPresence = float.PositiveInfinity;
		var maxPresence = float.NegativeInfinity;
		
		for (var i = 0; i < borderPointsCount - 1; i++)
		{
			for (var j = i + 1; j < borderPointsCount; j++)
			{
				var paths = GetAllPathsBetweenNodes(graph, i, j);
				foreach (var path in paths)
				{
					for (var x = 0; x < path.Length - 1; x++)
					{
						var edge = new DelaunayTriangulator.Edge(path[x], path[x + 1]);
						var index = edges.FindIndex((otherEdge) => otherEdge.Equals(edge));
						edgeWeights[index] += 1;
						minPresence = Mathf.Min(minPresence, edgeWeights[index]);
						maxPresence = Mathf.Max(maxPresence, edgeWeights[index]);
					}
				}
			}
		}

		for (int i = 0; i < edges.Count; i++)
		{
			edgeWeights[i] = Mathf.Remap(edgeWeights[i], minPresence, maxPresence, 0, 1);
		}
		
		return edgeWeights;
	}
	
	private List<int[]> GetAllPathsBetweenNodes(bool[][] graph, int startIndex, int endIndex)
	{
		if(startIndex == endIndex) return null;
		var paths = new List<int[]>();
		FindAllPaths(graph, startIndex, endIndex, new(), paths, new());
		return paths;
	}
	
	private void FindAllPaths(bool[][] graph, int current, int target, List<int> currentPath, List<int[]> paths, HashSet<int> visited)
	{
		visited.Add(current);
		currentPath.Add(current);
		
		if (current == target)
		{
			paths.Add(currentPath.ToArray());
			return;
		}
		else
		{
			for (var i = 0; i < graph.Length; ++i)
			{
				if(graph[current][i] && !visited.Contains(i))
				{
					FindAllPaths(graph, i, target, currentPath, paths, visited);
				}
			}
		}
		
		visited.Remove(current);
		currentPath.RemoveAt(currentPath.Count - 1);
	}
	
	private HashSet<DelaunayTriangulator.Edge> RemoveRandomEdges(IEnumerable<DelaunayTriangulator.Triangle> triangles, int borderPointsCount)
	{
		var visitedEdges = new HashSet<DelaunayTriangulator.Edge>();
		var removedEdges = new HashSet<DelaunayTriangulator.Edge>();

		foreach (var triangle in triangles)
		{
			var edge1 = new DelaunayTriangulator.Edge(triangle.FirstIndex, triangle.SecondIndex);
			var edge2 = new DelaunayTriangulator.Edge(triangle.FirstIndex, triangle.ThirdIndex);
			var edge3 = new DelaunayTriangulator.Edge(triangle.SecondIndex, triangle.ThirdIndex);
			
			var removedEdge = false;
			
			foreach (var edge in new[] { edge1, edge2, edge3 })
			{
				if(edge.StartIndex < borderPointsCount && edge.EndIndex < borderPointsCount)
				{
					removedEdge = true;
					removedEdges.Add(edge);
				}
			}
			
			foreach (var edge in new[] { edge1, edge2, edge3 })
			{
				if(removedEdges.Contains(edge))
				{
					removedEdge = true;
					continue;
				}
				if(visitedEdges.Contains(edge)) continue;
				if(!removedEdge && GD.Randf() < RemoveEdgeChance)
				{
					removedEdges.Add(edge);
					removedEdge = true;
					continue;
				}
				
				visitedEdges.Add(edge);
			}
		}
		
		return visitedEdges;
	}
	
	private IEnumerable<Vector2> GetCurveEdgePoints(Vector2 start, Vector2 end)
	{
		var direction = end - start;
		var step = direction.Length() / 8;
		var angle = Mathf.Pi / 4;
		direction = direction.Normalized();
		
		var startRandomOut = start + (direction * (float) GD.RandRange(step, 3f * step)).Rotated((float)GD.RandRange(-angle, angle));
		ClampPointToRect(ref startRandomOut);
		
		var endRandomIn = end - (direction * (float) GD.RandRange(step, 3f * step)).Rotated((float)GD.RandRange(-angle, angle));
		ClampPointToRect(ref endRandomIn);
		
		for (var i = 0; i < k_BezierResolution; ++i)
		{
			yield return MathExtensions.CubicBezier(start, startRandomOut, endRandomIn, end, (float)i / k_BezierResolution);
		}
	}
	
	private void ClampPointToRect(ref Vector2 point)
	{
		if (point.X < 0)
		{
			point.X *= -1;
		}
		
		if (point.X > ChunkSize.X)
		{
			point.X = 2 * ChunkSize.X - point.X;
		}
		
		if (point.Y < 0)
		{
			point.Y *= -1;
		}
		
		if (point.Y > ChunkSize.Y)
		{
			point.Y = 2 * ChunkSize.Y - point.Y;
		}
	}
	
	public readonly struct Chunk
	{
		public IReadOnlyList<CellType> ChunkCells { get; }
		public IReadOnlyList<Vector2I> EdgeCells { get; }
		public IReadOnlyList<Vector2I> PointOfInterestCells { get; }

		public Chunk(
			IReadOnlyList<CellType> chunkCells,
			IReadOnlyList<Vector2I> edgeCells,
			IReadOnlyList<Vector2I> pointOfInterestCells
			)
		{
			ChunkCells = chunkCells;
			EdgeCells = edgeCells;
			PointOfInterestCells = pointOfInterestCells;
		}
	}
}
