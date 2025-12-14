using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGrid
{
    int Width { get; }
    int Height { get; }
    float CellSize { get; }

    event Action OnEmpty;
    event Action OnClear;
    event Func<UniTask> OnClearAsync;

    void Initialize(GridData levelData);

    AbstractBlock GetBlock(int x, int y);
    Vector3 GetPosition(int x, int y);
    Vector2Int GetGridPos(AbstractBlock block);
    Vector2Int GetGridPosFromWorld(Vector3 worldPos);

    void SetBlock(int x, int y, AbstractBlock block);

    UniTask DropDown();
    UniTask<bool> TryMoveBlock(AbstractBlock block, Vector2Int dir);

    UniTask DestroyComponents(List<List<Vector2Int>> components);
    UniTask ClearAsync();
    void Clear();

    bool IsValid(Vector2Int pos);
}