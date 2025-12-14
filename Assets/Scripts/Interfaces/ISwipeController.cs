using System;
using UnityEngine;

public interface ISwipeController
{
    public event Action<Vector2, Vector2Int> OnSwipe;
}
