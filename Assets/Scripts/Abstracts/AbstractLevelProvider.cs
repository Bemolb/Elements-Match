using Newtonsoft.Json;
using UnityEngine;

public abstract class AbstractLevelProvider : ScriptableObject
{
    public abstract GridData GetLevelGrid(int level);

    protected void LevelNotFound(int level)
    {
        Debug.LogError($"Level {level} not found.");
    }

    protected GridData DesirializeLevel(int level, TextAsset jsonText)
    {
        try
        {
            return JsonConvert.DeserializeObject<GridData>(jsonText.text);
        }
        catch (JsonException ex)
        {
            Debug.LogError($"Failed to deserialize level {level}: {ex.Message}");
            return null;
        }
    }
}
