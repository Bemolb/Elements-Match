using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;

public class LevelManager : MonoBehaviour
{
    private LevelData _currentLevel;

    [Inject] private INormalizationService _normalizationService;
    [Inject] private ISaveService _saveService;
    [Inject] private IGrid _grid;
    [Inject] private IGridPopulationService _gridPopulator;
    [Inject] private ISwipeController _inputController;
    [Inject] private AbstractLevelProvider _levelProvider;

    public ReactiveProperty<int> CurrentMoveCount { get; private set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> CurrentLevelNumber { get; private set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<LevelManagerState> State { get; private set; } = new ReactiveProperty<LevelManagerState>(LevelManagerState.Uninitialized);

    public void Initialize()
    {
        if (_grid == null || _gridPopulator == null || _inputController == null || _levelProvider == null || _saveService == null || _normalizationService == null)
        {
            Debug.LogError("One or more dependencies are not injected properly in LevelManager.");
            State.Value = LevelManagerState.Error;
            return;
        }

        _grid.OnEmpty += HandleNextLevel;
        _inputController.OnSwipe += HandleSwipe;

        State.Value = LevelManagerState.Initialized;
    }
    private void OnDestroy()
    {
        if (_grid != null)
            _grid.OnEmpty -= HandleNextLevel;
        if (_inputController != null)
            _inputController.OnSwipe -= HandleSwipe;
    }

    private void HandleSwipe(Vector2 startPos, Vector2Int dir) =>
        HandleSwipeAsync(startPos, dir).Forget();

    private async UniTaskVoid HandleSwipeAsync(Vector2 startPos, Vector2Int dir)
    {
        try
        {
            if (Camera.main == null) 
                return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(startPos);
            Vector2Int gridPos = _grid.GetGridPosFromWorld(worldPos);

            if (gridPos.x == -1) 
                return;

            AbstractBlock block = _grid.GetBlock(gridPos.x, gridPos.y);

            if (block == null || block.State != BlockState.Idle) 
                return;

            if (await _grid.TryMoveBlock(block, dir))
            {
                CurrentMoveCount.Value++;
                await _normalizationService.Normalize(_grid);
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing swipe: {ex.Message}");
        }
    }

    public void SaveCurrentState()
    {
        _currentLevel.MoveCount = CurrentMoveCount.Value;
        _currentLevel.VisualNumber = CurrentLevelNumber.Value;

        _saveService.SaveGrid(_grid);
        _saveService.SaveLevel(_currentLevel);
    }    

    private void HandleNextLevel()
    {
        _currentLevel.LevelId++;
        CurrentLevelNumber.Value++;
        LoadLevel();
    }

    public void Restart() =>
        LoadLevel();
    public void NextLevel() =>
        HandleNextLevel();

    public void Load()
    {
        State.Value = LevelManagerState.Loading;

        var savedGridData = _saveService.LoadGrid();
        _currentLevel = _saveService.LoadLevel() ?? new LevelData { VisualNumber = 1, LevelId = 1 };
        CurrentMoveCount.Value = _currentLevel.MoveCount;
        CurrentLevelNumber.Value = _currentLevel.VisualNumber;

        if (savedGridData != null)
        {
            _grid.Initialize(savedGridData);
            _gridPopulator.Populate(savedGridData, _grid);
            _ = _normalizationService.Normalize(_grid);
        }
        else
        {
            LoadLevel();
        }

        State.Value = LevelManagerState.Loaded;
    }

    private void LoadLevel()
    {
        var data = _levelProvider.GetLevelGrid(_currentLevel.LevelId);

        if (data == null)
        {
            _currentLevel.LevelId = 1;
            data = _levelProvider.GetLevelGrid(_currentLevel.LevelId);

            if (data == null)
            {
                Debug.LogError("Failed to load even the first level. Check level configurations.");
                return;
            }
        }

        CurrentMoveCount.Value = 0;
        _grid.Clear();
        _grid.Initialize(data);
        _gridPopulator.Populate(data, _grid);
        _ = _normalizationService.Normalize(_grid);
    }
}