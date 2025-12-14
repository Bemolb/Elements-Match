using MessagePack;
using System;

[MessagePackObject]
[Serializable]
public class LevelData
{
    [Key(0)]
    public int LevelId { get; set; }
    [Key(1)]
    public int VisualNumber { get; set; }
    [Key(2)]
    public int MoveCount { get; set; }
}
