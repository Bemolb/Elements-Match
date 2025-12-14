using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

public class Grid : IGrid
{
    private AbstractBlock[,] _gridBlocks;
    private int _width;
    private int _height;
    private float _cellSize;
    private Vector3 _bottomIndent;
    private CancellationTokenSource _cts;

    [Inject] private GameConfig _gameConfig;

    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;

    public event Action OnEmpty;
    public event Action OnClear; 
    public event Func<UniTask> OnClearAsync;

    public void Initialize(GridData levelData)
    {
        _width = levelData.Width;
        _height = levelData.Height;
        _gridBlocks = new AbstractBlock[_width, _height];

        _cts = new CancellationTokenSource();

        CalculateSize();
    }

    public void Clear()
    {
        if(_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        OnClear?.Invoke();
        _gridBlocks = new AbstractBlock[_width, _height];
    }

    public async UniTask ClearAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (OnClearAsync != null)
        {
            var tasks = OnClearAsync.GetInvocationList()
                .Cast<Func<UniTask>>()
                .Select(func => func.Invoke());

            await UniTask.WhenAll(tasks);
        }

        _gridBlocks = new AbstractBlock[_width, _height];
    }

    public async UniTask DropDown()
    {
        List<UniTask> dropTasks = new();

        for (int x = 0; x < Width; x++)
        {
            int emptyY = 0;
            for (int y = 0; y < Height; y++)
            {
                if (_gridBlocks[x, y] == null)
                {
                    emptyY++;
                }
                else if (emptyY > 0)
                {
                    Vector2Int newPos = new(x, y - emptyY);
                    AbstractBlock block = _gridBlocks[x, y];
                    _gridBlocks[newPos.x, newPos.y] = block;
                    _gridBlocks[x, y] = null;
                    dropTasks.Add(block.DropDown(newPos));
                }
            }
        }

        if (dropTasks.Count > 0)
            await UniTask.WhenAll(dropTasks)
                .AttachExternalCancellation(_cts.Token);
    }

    public async UniTask DestroyComponents(List<List<Vector2Int>> components)
    {
        List<UniTask> tasks = new();
        HashSet<Vector2Int> toDestroy = new();

        foreach (var comp in components)
        {
            foreach (var pos in comp)
            {
                var block = _gridBlocks[pos.x, pos.y];
                tasks.Add(block.DestroyBlock());
                toDestroy.Add(pos);
            }
        }

        await UniTask.WhenAll(tasks)
            .AttachExternalCancellation(_cts.Token);

        foreach(var pos in toDestroy)
            _gridBlocks[pos.x, pos.y] = null;

        if (IsEmpty())
            OnEmpty?.Invoke();
    }

    public Vector2Int GetGridPosFromWorld(Vector3 worldPos)
    {
        Vector2 localPos = (Vector2)(worldPos - _bottomIndent);

        int x = Mathf.FloorToInt(localPos.x / _cellSize);
        int y = Mathf.FloorToInt(localPos.y / _cellSize);

        if (x >= 0 && x < _width && y >= 0 && y < _height)
            return new Vector2Int(x, y);

        return new Vector2Int(-1, -1);
    }

    public Vector2Int GetGridPos(AbstractBlock block)
    {
        float px = (block.Position.x - _bottomIndent.x) / _cellSize;
        float py = (block.Position.y - _bottomIndent.y) / _cellSize;

        int ix = Mathf.FloorToInt(px);
        int iy = Mathf.FloorToInt(py);

        ix = Mathf.Clamp(ix, 0, _width - 1);
        iy = Mathf.Clamp(iy, 0, _height - 1);

        return new Vector2Int(ix, iy);
    }

    public async UniTask<bool> TryMoveBlock(AbstractBlock block, Vector2Int dir)
    {
        Vector2Int pos = GetGridPos(block);
        Vector2Int targetPos = pos + dir;

        if (block.State != BlockState.Idle)
            return false;

        if (!IsValid(targetPos))
            return false;

        if (dir == Vector2Int.up && _gridBlocks[targetPos.x, targetPos.y] == null)
            return false;

        AbstractBlock targetBlock = _gridBlocks[targetPos.x, targetPos.y];

        List<UniTask> moveTasks = new();

        if (targetBlock != null)
        {
            if (targetBlock.State != BlockState.Idle)
                return false;
            else
                moveTasks.Add(targetBlock.MoveTo(pos));
        }

        moveTasks.Add(block.MoveTo(targetPos));

        _gridBlocks[pos.x, pos.y] = targetBlock;
        _gridBlocks[targetPos.x, targetPos.y] = block;

        await UniTask.WhenAll(moveTasks)
            .AttachExternalCancellation(_cts.Token)
            .SuppressCancellationThrow();

        return true;
    }

    private void CalculateSize()
    {
        if (Camera.main == null)
            return;

        var screenHeightWorld = Camera.main.orthographicSize * 2f;
        var screenWidthWorld = screenHeightWorld * Camera.main.aspect;
        var availableHeightWorld = screenHeightWorld - _gameConfig.GridOffsetFromBottom - _gameConfig.GridOffsetFromTop;

        var maxCellSizeWidth = screenWidthWorld * _gameConfig.GridPercent / _width;
        var maxCellSizeHeight = availableHeightWorld * _gameConfig.GridPercent / _height;

        _cellSize = Mathf.Min(maxCellSizeWidth, maxCellSizeHeight);

        var cameraBottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        var gridWidthWorld = _width * _cellSize;
        var centerX = Camera.main.transform.position.x - gridWidthWorld * .5f;

        _bottomIndent = new Vector3(centerX, cameraBottom + _gameConfig.GridOffsetFromBottom, 0);
    }

    private bool IsEmpty()
    {
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                if (_gridBlocks[x, y] != null) 
                    return false;

        return true;
    }

    public bool IsValid(Vector2Int pos) => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    public void SetBlock(int x, int y, AbstractBlock block) => _gridBlocks[x, y] = block;
    public AbstractBlock GetBlock(int x, int y) => _gridBlocks[x, y];
    public Vector3 GetPosition(int x, int y) => _bottomIndent + new Vector3((x + 0.5f) * _cellSize, (y + 0.5f) * _cellSize, 0);
}