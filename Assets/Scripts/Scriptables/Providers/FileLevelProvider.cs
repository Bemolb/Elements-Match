using UnityEngine;

[CreateAssetMenu(fileName = "FileLevelProvider", menuName = "Levels/File Level Provider")]
public class FileLevelProvider : AbstractLevelProvider
{
    [SerializeField] private string _pathPattern;
    public override GridData GetLevelGrid(int level)
    {
        string path = string.Format(_pathPattern, level);
        TextAsset jsonText = Resources.Load<TextAsset>(path);

        if (jsonText == null)
        {
            LevelNotFound(level);
            return null;
        }

        return DesirializeLevel(level, jsonText);
    }
}
