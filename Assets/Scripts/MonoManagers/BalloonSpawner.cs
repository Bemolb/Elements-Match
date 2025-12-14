using UnityEngine;
using Zenject;

public class BalloonSpawner : MonoBehaviour
{
    [Inject] private BalloonPool _pool;
    [Inject] private BalloonConfig _balloonConfig;

    private int _balloonsCount;

    void Start()
    {
        for (int i = 0; i < _balloonConfig.MaxBalloonCount; i++)
            SpawnBalloon();
    }

    private void SpawnBalloon()
    {
        var balloonObj = _pool.Get(); 

        if (balloonObj.TryGetComponent<IPoolable<GameObject>>(out var poolable))
            poolable.OnDestroy += OnBalloonDestroy;

        _balloonsCount++;
    }

    private void OnBalloonDestroy(GameObject balloon)
    {
        if (balloon.TryGetComponent<IPoolable<GameObject>>(out var poolable))
            poolable.OnDestroy -= OnBalloonDestroy;

        _balloonsCount--;

        if (_balloonsCount < _balloonConfig.MaxBalloonCount)
            SpawnBalloon();
    }
}