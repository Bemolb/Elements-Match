using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BlockFactory : IFactory<int, AbstractBlock>
{
    private readonly DiContainer _container;
    private readonly AbstractBlockProvider _blockProvider;

    private readonly GameObject _basePoolHolder;
    private readonly Dictionary<int, Stack<AbstractBlock>> _pools = new();
    private readonly Dictionary<int, GameObject> _poolHolders = new();

    [Inject]
    public BlockFactory(DiContainer container, AbstractBlockProvider blockProvider)
    {
        _container = container;
        _blockProvider = blockProvider;
        _basePoolHolder = new GameObject(ToString());
    }

    public AbstractBlock Create(int type)
    {
        AbstractBlock block; 

        if (_pools.TryGetValue(type, out var pool) && pool.Count > 0)
            block = pool.Pop();
        else
            block = CreateNew(type);

        if(block != null)
            block.gameObject.SetActive(true);

        if (block is IPoolable<AbstractBlock> poolable)
            poolable.OnGetFromPool();

        return block;
    }

    private void Return(AbstractBlock block)
    {
        if (block == null || block.gameObject == null) 
            return;

        if (block is IPoolable<AbstractBlock> poolable)
            poolable.OnReturnToPool();

        block.gameObject.SetActive(false);

        if (!_pools.TryGetValue(block.Type, out var pool))
        {
            pool = new Stack<AbstractBlock>();
            _pools[block.Type] = pool;
        }

        pool.Push(block);
    }

    private AbstractBlock CreateNew(int type)
    {
        var prefab = _blockProvider.GetBlockPrefab(type);

        if (prefab == null)
        {
            Debug.LogWarning("Block prefab is null.");
            return null;
        }

        if (!_poolHolders.ContainsKey(type))
        {
            var holder = new GameObject(ToString() + type.ToString());
            holder.transform.SetParent(_basePoolHolder.transform);
            _poolHolders.Add(type, holder);
        }

        var poolHolder = _poolHolders[type].transform;

        var instance = _container.InstantiatePrefab(prefab, poolHolder);
        var block = instance.GetComponent<AbstractBlock>();

        if (block is IPoolable<AbstractBlock> poolable)
            poolable.OnDestroy += Return;

        return block;
    }
}