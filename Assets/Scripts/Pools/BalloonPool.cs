using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BalloonPool
{
    private readonly Dictionary<int, Stack<GameObject>> _pools = new();
    private readonly Dictionary<int, GameObject> _poolHolders = new();

    private readonly List<GameObject> _prefabs;
    private readonly GameObject _basePoolHolder;
    private readonly DiContainer _container;

    [Inject]
    public BalloonPool(DiContainer container, BalloonConfig gameConfig)
    {
        _container = container;
        _prefabs = gameConfig.BalloonPrefabs;

        if (_basePoolHolder == null)
            _basePoolHolder = new GameObject(ToString());

        InitPools(gameConfig.InitialBalloonPoolSize);
    }

    private void InitPools(int initSize)
    {
        int variation = 0;

        foreach (var prefab in _prefabs)
        {
            if (!_poolHolders.ContainsKey(variation))
            {
                var holder = new GameObject(ToString() + variation.ToString());
                holder.transform.SetParent(_basePoolHolder.transform);
                _poolHolders.Add(variation, holder);
                _pools.Add(variation, new Stack<GameObject>());
            }

            var pool = _pools[variation];

            for (int i = 0; i < initSize; i++)
            {
                var obj = CreateNew(variation);
                obj.SetActive(false);
                pool.Push(obj);
            }
            variation++;
        }
    }

    private GameObject CreateNew(int variation)
    {
        var prefab = _prefabs[variation];
        var holder = _poolHolders[variation];

        var instance = _container.InstantiatePrefab(prefab, holder.transform);
        var variantComponent = instance.AddComponent<PoolableVariant>();
        variantComponent.Variation = variation;

        if (instance.TryGetComponent<IPoolable<GameObject>>(out var poolable))
            poolable.OnDestroy += Return;

        return instance;
    }

    public GameObject Get()
    {
        GameObject obj;

        var variation = Random.Range(0, _prefabs.Count);

        if (_pools.TryGetValue(variation, out var pool) && pool.Count > 0)
            obj = pool.Pop();
        else
            obj = CreateNew(variation);

        obj.SetActive(true);
        
        if (obj.TryGetComponent<IPoolable<GameObject>>(out var poolable))
            poolable.OnGetFromPool();

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) 
            return;
        
        if (obj.TryGetComponent<IPoolable<GameObject>>(out var poolable))
            poolable.OnReturnToPool();

        if (obj.TryGetComponent<PoolableVariant>(out var variant))
        {
            int variation = variant.Variation;

            if (_pools.ContainsKey(variation))
            {
                obj.SetActive(false);
                _pools[variation].Push(obj);
            }
            else
            {
                Debug.LogError($"Pool for variation {variation} not found!");
                Object.Destroy(obj);
            }
        }
        else
        {
            Debug.LogError("BalloonVariant component not found on object!");
            Object.Destroy(obj);
        }
    }
}