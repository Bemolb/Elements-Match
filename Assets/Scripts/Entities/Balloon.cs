using UnityEngine;
using Zenject;

public class Balloon : MonoBehaviour, IPoolable<GameObject>
{
    private float _speed;
    private Vector2 _direction;
    private float _initialY;

    [Inject] private BalloonConfig _config;

    public event System.Action<GameObject> OnDestroy;

    public void OnGetFromPool()
    {
        _speed = Random.Range(_config.MinSpeed, _config.MaxSpeed);
        _direction = Random.value > 0.5f ? Vector2.right : Vector2.left;
        var minInitialY = Camera.main.orthographicSize * -(1 - _config.BottomLiftPercent);
        _initialY = Random.Range(minInitialY, Camera.main.orthographicSize);

        var screenWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        var startX = _direction.x > 0 ? -screenWidth * .5f - _config.BorderMargin : screenWidth * .5f + _config.BorderMargin;
        transform.position = new Vector3(startX, _initialY, 0);
    }

    public void OnReturnToPool() {}

    void Update()
    {
        transform.position += (Vector3)(_speed * Time.deltaTime * _direction);
        var yOffset = Mathf.Sin(Time.time * _speed) * _config.Amplitude;
        transform.position = new Vector3(transform.position.x, _initialY + yOffset, 0);

        var screenWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;

        var leftBound = -screenWidth * .5f - _config.BorderMargin;
        var rightBound = screenWidth * .5f + _config.BorderMargin;

        if ((_direction.x > 0 && transform.position.x > rightBound) ||
            (_direction.x < 0 && transform.position.x < leftBound))
                OnDestroy?.Invoke(gameObject);
    }
}