using UnityEngine;
using Zenject;

public class ProvidersInstaller : MonoInstaller
{
    [SerializeField] private AbstractLevelProvider _levelProvider;
    [SerializeField] private AbstractBlockProvider _blockProvider;
    public override void InstallBindings()
    {
        Container.BindInstance(_blockProvider).AsSingle();
        Container.BindInstance(_levelProvider).AsSingle();
    }
}
