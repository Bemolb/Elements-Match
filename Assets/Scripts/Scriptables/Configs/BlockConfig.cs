using UnityEngine;

[CreateAssetMenu(fileName = "BlockConfig", menuName = "Configs/Block Config")]
public class BlockConfig : ScriptableObject
{
    [SerializeField] private float _moveDuration;

    public float MoveDuration => _moveDuration;
}
