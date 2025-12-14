using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class OptimizedComponentFinder : IComponentFinder
{
    [Inject] private GameConfig _gameConfig;

    public List<List<Vector2Int>> FindComponents(IGrid grid)
    {
        NativeArray<int> gridData = new(grid.Width * grid.Height, Allocator.TempJob);

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var block = grid.GetBlock(x, y);
                gridData[y * grid.Width + x] = block != null ? block.Type : _gameConfig.EmptyBlockType;
            }
        }

        NativeList<int2> allPositions = new(Allocator.TempJob);
        NativeList<int> componentLengths = new(Allocator.TempJob);

        var job = new FindComponentsJob
        {
            GridData = gridData,
            Width = grid.Width,
            Height = grid.Height,
            AllPositions = allPositions,
            ComponentLengths = componentLengths
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        List<List<Vector2Int>> components = new();
        int posIndex = 0;

        for (int i = 0; i < componentLengths.Length; i++)
        {
            int length = componentLengths[i];
            List<Vector2Int> comp = new(length);

            for (int j = 0; j < length; j++)
            {
                int2 pos = allPositions[posIndex++];
                comp.Add(new Vector2Int(pos.x, pos.y));
            }

            components.Add(comp);
        }

        gridData.Dispose();
        allPositions.Dispose();
        componentLengths.Dispose();

        return components;
    }
}