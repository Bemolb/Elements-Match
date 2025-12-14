using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteToCamScaler : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ScaleToFitCamera();
    }

    void ScaleToFitCamera()
    {
        if (Camera.main == null) 
            return;

        var screenHeight = Camera.main.orthographicSize * 2f;
        var screenWidth = screenHeight * Screen.width / Screen.height;

        var spriteSize = _spriteRenderer.sprite.bounds.size;

        var scaleX = screenWidth / spriteSize.x;
        var scaleY = screenHeight / spriteSize.y;
        var scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new (scale, scale, 1f);
        transform.position = (Vector2)Camera.main.transform.position;
    }
}