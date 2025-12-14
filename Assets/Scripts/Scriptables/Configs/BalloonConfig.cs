using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BalloonConfig", menuName = "Configs/Balloon Config")]
public class BalloonConfig : ScriptableObject
{
    [SerializeField] private List<GameObject> _balloonPrefabs;
    [SerializeField][Min(0)] private int _initialBalloonPoolSize;
    [SerializeField][Min(.1f)] private float _minSpeed;
    [SerializeField][Min(.1f)] private float _maxSpeed;
    [SerializeField][Range(0f, 1f)] private float _bottomLiftPercent;
    [SerializeField] private float _amplitude;
    [SerializeField][Min(0)] private float _borderMargin;
    [SerializeField][Min(1)] private int _maxBalloonCount;

    public List<GameObject> BalloonPrefabs => _balloonPrefabs;
    public int InitialBalloonPoolSize => _initialBalloonPoolSize;
    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float BottomLiftPercent => _bottomLiftPercent;
    public float Amplitude => _amplitude;
    public float BorderMargin => _borderMargin;
    public int MaxBalloonCount => _maxBalloonCount;
}
