using System.Collections.Generic;
using UnityEngine;

public interface IComponentFinder
{
    List<List<Vector2Int>> FindComponents(IGrid grid);
}
