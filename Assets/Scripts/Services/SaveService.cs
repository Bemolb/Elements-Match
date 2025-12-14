using MessagePack;
using System;
using UnityEngine;
using Zenject;

public class SaveService : ISaveService
{
    private const string LEVEL_KEY = "LevelState";
    private const string GRID_KEY = "GridState";

    [Inject] private GameConfig _gameConfig;

    public void SaveGrid(IGrid grid)
    {
        if (grid.Width <= 0)
        {
            PlayerPrefs.DeleteKey(GRID_KEY);
            return;
        }

        GridData data = new()
        {
            Width = grid.Width,
            Height = grid.Height,
            Grid = new int[grid.Height][]
        };

        for (int y = 0; y < data.Height; y++)
        {
            data.Grid[y] = new int[data.Width];

            for (int x = 0; x < data.Width; x++)
            {
                var block = grid.GetBlock(x, data.Height - 1 - y);
                data.Grid[y][x] = block != null ? block.Type : _gameConfig.EmptyBlockType;
            }
        }

        Save(data, GRID_KEY);
    }

    public void SaveLevel(LevelData level) =>
        Save(level, LEVEL_KEY);

    public void Save<T>(T obj, string key) where T : class
    {
        byte[] bytes = MessagePackSerializer.Serialize(obj);

        PlayerPrefs.SetString(key, Convert.ToBase64String(bytes));
        PlayerPrefs.Save();
    }

    public GridData LoadGrid() =>
        Load<GridData>(GRID_KEY);

    public LevelData LoadLevel() =>
        Load<LevelData>(LEVEL_KEY);

    public T Load<T>(string key) where T : class
    {
        string base64 = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(base64)) 
            return null;

        byte[] bytes = Convert.FromBase64String(base64);

        return MessagePackSerializer.Deserialize<T>(bytes);
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
