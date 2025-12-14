using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public abstract class AbstractBlock : MonoBehaviour, IPoolable<AbstractBlock>
{
    public int Type { get; protected set; }
    public BlockState State { get; protected set; } = BlockState.Idle;
    public IGrid ParentGrid { get; protected set; }
    public Vector3 Position => transform.position;

    public event Action<AbstractBlock> OnDestroy;

    [Inject] protected BlockConfig _config;

    protected CancellationTokenSource _cts;

    public virtual void Initialize(int type, IGrid parentGrid)
    {
        Type = type;
        ParentGrid = parentGrid;
        ParentGrid.OnClear += Destroy;
    }

    protected virtual void Awake()
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    public abstract UniTask MoveTo(Vector2Int targetPos);
    public abstract UniTask DropDown(Vector2Int targetPos);

    protected void UpdateSortingOrder(Vector2Int position)
    {
        if (TryGetComponent<SpriteRenderer>(out var renderer))
            renderer.sortingOrder = position.y * ParentGrid.Width + position.x;
    }

    public abstract UniTask DestroyBlock();

    public virtual void Destroy()
    {
        if (ParentGrid != null)
        {
            ParentGrid.OnClear -= Destroy;
            ParentGrid = null;
        }

        OnDestroy?.Invoke(this);
    }

    public virtual void OnGetFromPool()
    {
        State = BlockState.Idle;

        _cts ??= CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    public virtual void OnReturnToPool()
    {
        State = BlockState.Pooled;

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
    }
}