using System;
using System.Collections.Generic;
using Godot;

public class NodePool<T> where T : Node
{
	private event Func<T> CreateFunc;
	private event Action<T> GetCallback;
	private event Action<T> ReturnCallback;
	
	private readonly Stack<T> m_Pool = new();

	public int PoolCapacity { get; private set; }
	public int FreeNodesCount => m_Pool.Count;
	
	public NodePool(Func<T> createFunc, int initialCapacity = 100, 
		Action<T> getCallback = null, Action<T> returnCallback = null)
	{
		PoolCapacity = initialCapacity;
		
		CreateFunc = createFunc;
		GetCallback = getCallback;
		ReturnCallback = returnCallback;
		
		for (var i = 0; i < PoolCapacity; ++i)
		{
			Return(CreateInstance());
		}
	}
	
	private T CreateInstance()
	{
		return CreateFunc?.Invoke();
	}
	
	public bool TryGet(out T instance)
	{
		if(FreeNodesCount == 0)
		{
			Return(CreateInstance());
			PoolCapacity++;
		}
		
		instance = m_Pool.Pop();
		
		if(instance != null)
			GetCallback?.Invoke(instance);
		
		return instance != null;
	}
	
	public void Return(T instance)
	{
		m_Pool.Push(instance);
		ReturnCallback?.Invoke(instance);
	}
}