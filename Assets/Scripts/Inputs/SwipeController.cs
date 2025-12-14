using System;
using UnityEngine;
using Zenject;

public class SwipeController : MonoBehaviour, ISwipeController
{
    public event Action<Vector2, Vector2Int> OnSwipe;

    [Inject] private GameConfig _gameConfig;

    private Controls _controls;
    private Vector2 _startPos;
    private bool _isDragging;

    private float _lastSwipeTime;

    void Awake()
    {
        _controls = new Controls();
        _controls.Gameplay.Press.started += ctx => StartDrag();
        _controls.Gameplay.Press.canceled += ctx => EndDrag();
    }

    private void StartDrag()
    {
        _startPos = _controls.Gameplay.Position.ReadValue<Vector2>();
        _isDragging = true;
    }

    private void EndDrag()
    {
        if (!_isDragging) 
            return;

        if (Time.time - _lastSwipeTime < _gameConfig.SwipeCooldown)
            return;

        var endPos = _controls.Gameplay.Position.ReadValue<Vector2>();
        var delta = endPos - _startPos;

        if (delta.magnitude > 50)
            OnSwipe?.Invoke(_startPos, GetDirection(delta));

        _isDragging = false;
    }

    private Vector2Int GetDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? Vector2Int.right : Vector2Int.left;

        return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
        _controls.Gameplay.Press.started -= ctx => StartDrag();
        _controls.Gameplay.Press.canceled -= ctx => EndDrag();
    }

    void OnDestroy()
    {
        _controls.Dispose();
    }
}