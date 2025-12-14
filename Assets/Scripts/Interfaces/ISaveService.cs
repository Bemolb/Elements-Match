public interface ISaveService
{
    void SaveGrid(IGrid grid);
    void SaveLevel(LevelData level);
    void Save<T>(T obj, string key) where T : class;
    GridData LoadGrid();
    LevelData LoadLevel();
    T Load<T>(string key) where T : class;
    void ResetAll();
}
