using UnityEngine;
using Zenject;

public class ConfigsInstaller : MonoInstaller
{
    [SerializeField] private BlockConfig _blockConfig;
    [SerializeField] private BalloonConfig _balloonConfig;
    [SerializeField] private GameConfig _gameConfig;
    public override void InstallBindings()
    {
        Container.BindInstance(_gameConfig).AsSingle();
        Container.BindInstance(_blockConfig).AsSingle();
        Container.BindInstance(_balloonConfig).AsSingle();
    }
}
