using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListLevelProvider", menuName = "Levels/List Level Provider")]
public class ListLevelProvider : AbstractLevelProvider
{
    [SerializeField] private List<TextAsset> _levels = new();

    public override GridData GetLevelGrid(int level)
    {
        if (level >= _levels.Count)
        {
            LevelNotFound(level);
            return null;
        }

        TextAsset jsonText = _levels[level - 1];

        if (jsonText == null)
        {
            LevelNotFound(level);
            return null;
        }

        return DesirializeLevel(level, jsonText);
    }
}
