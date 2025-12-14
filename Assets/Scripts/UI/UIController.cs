using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIController : MonoBehaviour
{
    [Inject] private LevelManager _levelManager;

    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private TextMeshProUGUI _moveCounter;
    [SerializeField] private TextMeshProUGUI _levelNumber;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _restartButton;

    private IDisposable _moveCountSubscription;
    private IDisposable _levelNumberSubscription;

    private float _accumulatedTime = 0f;
    private int _frameCount = 0;
    private float _currentFps;
    private float _updateInterval = 0.1f;

    private void Start()
    {
        if (_nextButton != null)
            _nextButton.onClick.AddListener(HandleNextLevel);
        if (_restartButton != null)
            _restartButton.onClick.AddListener(HandleRestart);

        _moveCountSubscription = _levelManager.CurrentMoveCount.Subscribe(UpdateMoveCount)
            .AddTo(this);
        _levelNumberSubscription = _levelManager.CurrentLevelNumber.Subscribe(UpdateLevelNumber)
            .AddTo(this);
    }

    private void Update()
    {
        _accumulatedTime += Time.deltaTime;
        _frameCount++;

        if (_accumulatedTime >= _updateInterval)
        {
            _currentFps = _frameCount / _accumulatedTime;

            if (_fpsText != null)
                _fpsText.text = $"FPS: {_currentFps:F1}";

            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }

    private void OnDestroy()
    {
        _moveCountSubscription?.Dispose();
        _levelNumberSubscription?.Dispose();

        if (_nextButton != null)
            _nextButton.onClick.RemoveListener(HandleNextLevel);
        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(HandleRestart);
    }

    private void HandleNextLevel() =>
        _levelManager.NextLevel();

    private void HandleRestart() =>
        _levelManager.Restart();

    private void UpdateMoveCount(int moveCount)
    {
        if (_moveCounter != null)
            _moveCounter.text = moveCount.ToString();
    }

    private void UpdateLevelNumber(int number)
    {
        if (_levelNumber != null)
            _levelNumber.text = number.ToString();
    }
}