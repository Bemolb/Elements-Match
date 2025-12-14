using UnityEngine;

public abstract class AbstractBlockProvider : ScriptableObject
{
    public abstract AbstractBlock GetBlockPrefab(int type);
}
