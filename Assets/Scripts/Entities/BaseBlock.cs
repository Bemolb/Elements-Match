using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class BaseBlock : AbstractBlock
{
    protected Animator _animator;
    protected bool _destroyAnimComplete;
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    public override async UniTask MoveTo(Vector2Int targetPos)
    {
        UpdateSortingOrder(targetPos);
        var targetPos3 = ParentGrid.GetPosition(targetPos.x, targetPos.y);
        float duration = _config.MoveDuration;
        float time = 0;
        Vector3 startPos = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos3, time / duration);
            time += Time.deltaTime;
            try
            {
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        transform.position = targetPos3;
    }

    public override async UniTask DropDown(Vector2Int targetPos)
    {
        State = BlockState.Drop;
        await MoveTo(targetPos);
        State = BlockState.Idle;
    }

    public override async UniTask DestroyBlock()
    {
        State = BlockState.Destroy;

        if (_animator == null)
        {
            Destroy();
            return;
        }

        _destroyAnimComplete = false;
        _animator.SetTrigger("Destroy");

        try
        {
            await UniTask.WaitUntil(() => _destroyAnimComplete,
                cancellationToken: _cts.Token,
                cancelImmediately: true);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Destroy();
    }
    public override void OnGetFromPool()
    {
        base.OnGetFromPool();

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);

        }
    }
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();

        if (_animator != null)
            _animator.StopPlayback();
    }

    public void OnDestroyAnimationComplete() => _destroyAnimComplete = true;
}