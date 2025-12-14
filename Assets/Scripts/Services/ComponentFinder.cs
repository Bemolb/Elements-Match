using System.Collections.Generic;
using UnityEngine;
using System;

public class ComponentFinder : IComponentFinder
{
    private readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public List<List<Vector2Int>> FindComponents(IGrid grid)
    {
        List<List<Vector2Int>> components = new();
        bool[,] visited = new bool[grid.Width, grid.Height];

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (grid.GetBlock(x, y) != null && !visited[x, y])
                {
                    var component = BFS(grid, new Vector2Int(x, y), visited);

                    if (component.Count >= 3 && HasLineOfThree(component, grid.Width, grid.Height))
                        components.Add(component);
                }
            }
        }
        return components;
    }

    private List<Vector2Int> BFS(IGrid grid, Vector2Int start, bool[,] visited)
    {
        List<Vector2Int> component = new(grid.Width * grid.Height / 2);
        Stack<Vector2Int> stack = new(grid.Width * grid.Height / 2);

        stack.Push(start);
        visited[start.x, start.y] = true;
        int type = grid.GetBlock(start.x, start.y).Type;

        while (stack.Count > 0)
        {
            Vector2Int pos = stack.Pop();
            component.Add(pos);

            foreach (var dir in directions)
            {
                Vector2Int neighbor = pos + dir;

                if (grid.IsValid(neighbor) && grid.GetBlock(neighbor.x, neighbor.y)?.Type == type && !visited[neighbor.x, neighbor.y])
                {
                    visited[neighbor.x, neighbor.y] = true;
                    stack.Push(neighbor);
                }
            }
        }

        return component;
    }

    private bool HasLineOfThree(List<Vector2Int> component, int width, int height)
    {
        int[] rowCounts = new int[height];

        foreach (var p in component) 
            rowCounts[p.y]++;

        for (int y = 0; y < height; y++)
        {
            if (rowCounts[y] < 3) 
                continue;

            int[] xs = new int[rowCounts[y]];
            int idx = 0;

            foreach (var p in component)
                if (p.y == y) 
                    xs[idx++] = p.x;

            Array.Sort(xs);

            for (int i = 0; i < xs.Length - 2; i++)
                if (xs[i + 1] == xs[i] + 1 && xs[i + 2] == xs[i] + 2)
                    return true;
        }

        int[] colCounts = new int[width];

        foreach (var p in component) 
            colCounts[p.x]++;

        for (int x = 0; x < width; x++)
        {
            if (colCounts[x] < 3) 
                continue;

            int[] ys = new int[colCounts[x]];
            int idx = 0;

            foreach (var p in component)
                if (p.x == x) 
                    ys[idx++] = p.y;

            Array.Sort(ys);

            for (int i = 0; i < ys.Length - 2; i++)
                if (ys[i + 1] == ys[i] + 1 && ys[i + 2] == ys[i] + 2)
                    return true;
        }

        return false;
    }
}