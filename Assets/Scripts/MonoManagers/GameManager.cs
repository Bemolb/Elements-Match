using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] private GameConfig _gameConfig;
    [Inject] private LevelManager _levelManager;

    private IDisposable _levelInitializeSubscription;

    void Start()
    {
        Application.targetFrameRate = _gameConfig.TargetFrameRate;
        QualitySettings.vSyncCount = _gameConfig.VSyncCount;

        _levelInitializeSubscription = _levelManager.State.Subscribe(LevelStateChanged).AddTo(this);
        _levelManager.Initialize();
    }

    private void LevelStateChanged(LevelManagerState state)
    {
        switch(state)
        {
            case LevelManagerState.Initialized:
                _levelManager.Load();
                break;
            case LevelManagerState.Error:
                _levelInitializeSubscription?.Dispose();
                break;
            case LevelManagerState.Loaded:
                SaveLevel();
                break;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveLevel();
    }

    private void OnApplicationQuit()
    {
        SaveLevel();
    }

    private void SaveLevel()
    {
        if (_levelManager != null)
            _levelManager.SaveCurrentState();
    }

    private void OnDestroy()
    {
        _levelInitializeSubscription?.Dispose();
    }
}
