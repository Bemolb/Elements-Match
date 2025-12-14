using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockProvider", menuName = "Block/Block Provider")]
public class DefaultBlockProvider : AbstractBlockProvider
{
    [SerializeField] private BlockPrefabData[] _prefabs;

    private readonly Dictionary<int, AbstractBlock> _uniqueBlocks = new();

    public override AbstractBlock GetBlockPrefab(int type)
    {
        if (_uniqueBlocks.TryGetValue(type, out var prefab))
            return prefab;

        Debug.LogWarning($"Prefab for type {type} not found.");
        return null;
    }

    private void OnValidate()
    {
        if (_prefabs == null)
            return;

        _uniqueBlocks.Clear();
        HashSet<int> seenTypes = new();

        foreach (var data in _prefabs)
        {
            if (!seenTypes.Add(data.Type))
            {
                Debug.LogError($"Duplicate type {data.Type} in BlockProvider! Skipping.");
                continue;
            }

            _uniqueBlocks[data.Type] = data.Prefab;
        }
    }

    private void OnEnable()
    {
        OnValidate();
    }
}