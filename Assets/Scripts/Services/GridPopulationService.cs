using UnityEngine;
using Zenject;

public class GridPopulationService : IGridPopulationService
{
    [Inject] private IFactory<int, AbstractBlock> _blockFactory;
    [Inject] private GameConfig _gameConfig;

    public void Populate(GridData levelData, IGrid grid)
    {
        if (levelData == null) 
            return;

        var cellSize = grid.CellSize;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var type = levelData.Grid[grid.Height - 1 - y][x];

                if (type == _gameConfig.EmptyBlockType)
                    continue;

                var block = _blockFactory.Create(type);

                if (block == null)
                {
                    Debug.LogError("Null object returned from factory. Skip this cell.");
                    continue;
                }

                if (block.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    spriteRenderer.sortingOrder = y * grid.Width + x;

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    var sprite = spriteRenderer.sprite;
                    var border = sprite.border;

                    Vector2 textureSize = new(sprite.texture.width, sprite.texture.height);

                    var visibleWidthPx = textureSize.x - border.x - border.z;
                    var visibleHeightPx = textureSize.y - border.y - border.w;

                    if (visibleWidthPx <= 0 || visibleHeightPx <= 0)
                    {
                        visibleWidthPx = sprite.rect.width;
                        visibleHeightPx = sprite.rect.height;
                    }

                    float ppu = sprite.pixelsPerUnit;

                    Vector2 visibleSizeUnits = new(visibleWidthPx / ppu, visibleHeightPx / ppu);

                    var scaleX = cellSize / visibleSizeUnits.x;
                    var scaleY = cellSize / visibleSizeUnits.y;
                    var scaleFactor = Mathf.Min(scaleX, scaleY);

                    block.transform.localScale = new(scaleFactor, scaleFactor, 1f);
                }
                else
                {
                    block.transform.localScale = new(cellSize, cellSize, 1f);
                }

                block.Initialize(type, grid);
                block.transform.position = grid.GetPosition(x, y);
                grid.SetBlock(x, y, block);
            }
        }
    }
}