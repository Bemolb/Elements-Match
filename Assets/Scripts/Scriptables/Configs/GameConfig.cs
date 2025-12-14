using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Blocks")]
    [SerializeField] private int _emptyBlockType;

    [Header("Input")]
    [SerializeField] private float _swipeCooldown;

    [Header("Grid")]
    [SerializeField] private float _gridOffsetFromBottom = 3f;
    [SerializeField] private float _gridOffsetFromTop = 0f;
    [SerializeField][Range(0f, 1f)] private float _gridPercent = .8f;

    [Header("Graphics")]
    [SerializeField] private int _targetFrameRate = 60;
    [SerializeField] private int _vSyncCount = 0;

    public int EmptyBlockType => _emptyBlockType;
    public float SwipeCooldown => _swipeCooldown;
    public float GridOffsetFromBottom => _gridOffsetFromBottom;
    public float GridOffsetFromTop => _gridOffsetFromTop;
    public float GridPercent => _gridPercent;

    public int TargetFrameRate => _targetFrameRate;
    public int VSyncCount => _vSyncCount;
}
