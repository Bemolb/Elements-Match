using System;

public interface IPoolable<T>
{
    event Action<T> OnDestroy;
    void OnGetFromPool();
    void OnReturnToPool();
}