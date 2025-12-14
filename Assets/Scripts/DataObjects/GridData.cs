using MessagePack;
using System;

[MessagePackObject]
[Serializable]
public class GridData
{
    [Key(0)]
    public int Width { get; set; }
    [Key(1)]
    public int Height { get; set; }
    [Key(2)]
    public int[][] Grid { get; set; }
}