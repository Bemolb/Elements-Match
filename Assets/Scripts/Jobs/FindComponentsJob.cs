using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FindComponentsJob : IJob
{
    [ReadOnly] public NativeArray<int> GridData;
    public int Width;
    public int Height;
    public NativeList<int2> AllPositions;
    public NativeList<int> ComponentLengths;

    public static readonly int2[] Directions = new int2[4]
    {
        new(0, 1),  // up
        new(0, -1), // down
        new(-1, 0), // left
        new(1, 0)   // right
    };

    public void Execute()
    {
        NativeArray<bool> visited = new(Width * Height, Allocator.Temp);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = y * Width + x;

                if (GridData[index] != 0 && !visited[index])
                {
                    NativeList<int2> component = BFS(new int2(x, y), visited);

                    if (component.Length >= 3 && HasLineOfThree(component))
                    {
                        AllPositions.AddRange(component);
                        ComponentLengths.Add(component.Length);
                    }

                    component.Dispose();
                }
            }
        }

        visited.Dispose();
    }

    private NativeList<int2> BFS(int2 start, NativeArray<bool> visited)
    {
        NativeList<int2> component = new(Allocator.Temp);
        NativeQueue<int2> queue = new(Allocator.Temp);

        int startIndex = start.y * Width + start.x;
        int type = GridData[startIndex];
        queue.Enqueue(start);
        visited[startIndex] = true;

        while (!queue.IsEmpty())
        {
            int2 pos = queue.Dequeue();
            component.Add(pos);

            for (int d = 0; d < 4; d++)
            {
                int2 neighbor = pos + Directions[d];
                if (neighbor.x >= 0 && neighbor.x < Width && neighbor.y >= 0 && neighbor.y < Height)
                {
                    int nIndex = neighbor.y * Width + neighbor.x;
                    if (GridData[nIndex] == type && !visited[nIndex])
                    {
                        visited[nIndex] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        queue.Dispose();
        return component;
    }

    private bool HasLineOfThree(NativeList<int2> component)
    {
        if (component.Length < 3) 
            return false;

        int minY = int.MaxValue, maxY = int.MinValue;
        int minX = int.MaxValue, maxX = int.MinValue;
        for (int i = 0; i < component.Length; i++)
        {
            int2 p = component[i];
            minX = math.min(minX, p.x);
            maxX = math.max(maxX, p.x);
            minY = math.min(minY, p.y);
            maxY = math.max(maxY, p.y);
        }

        int rowCount = maxY - minY + 1;
        int colCount = maxX - minX + 1;

        NativeArray<NativeList<int>> rows = new(rowCount, Allocator.Temp);

        for (int i = 0; i < rowCount; i++)
            rows[i] = new NativeList<int>(component.Length, Allocator.Temp);

        for (int i = 0; i < component.Length; i++)
        {
            int2 p = component[i];
            rows[p.y - minY].Add(p.x);
        }

        for (int r = 0; r < rowCount; r++)
        {
            var xs = rows[r];

            if (xs.Length < 3) 
                continue;

            xs.Sort();

            for (int i = 0; i < xs.Length - 2; i++)
            {
                if (xs[i + 1] == xs[i] + 1 && xs[i + 2] == xs[i] + 2)
                {
                    DisposeNativeArrayOfLists(rows);
                    return true;
                }
            }
        }

        DisposeNativeArrayOfLists(rows);

        NativeArray<NativeList<int>> cols = new(colCount, Allocator.Temp);

        for (int i = 0; i < colCount; i++)
            cols[i] = new NativeList<int>(component.Length, Allocator.Temp);

        for (int i = 0; i < component.Length; i++)
        {
            int2 p = component[i];
            cols[p.x - minX].Add(p.y);
        }

        for (int c = 0; c < colCount; c++)
        {
            var ys = cols[c];

            if (ys.Length < 3) 
                continue;

            ys.Sort();

            for (int i = 0; i < ys.Length - 2; i++)
            {
                if (ys[i + 1] == ys[i] + 1 && ys[i + 2] == ys[i] + 2)
                {
                    DisposeNativeArrayOfLists(cols);
                    return true;
                }
            }
        }

        DisposeNativeArrayOfLists(cols);
        return false;
    }

    private void DisposeNativeArrayOfLists(NativeArray<NativeList<int>> array)
    {
        for (int i = 0; i < array.Length; i++)
            array[i].Dispose();

        array.Dispose();
    }
}