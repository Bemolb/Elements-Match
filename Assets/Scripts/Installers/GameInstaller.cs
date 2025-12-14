using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IFactory<int, AbstractBlock>>().To<BlockFactory>().AsSingle();
        Container.Bind<IGrid>().To<Grid>().AsSingle();
        Container.Bind<IGridPopulationService>().To<GridPopulationService>().AsSingle();
        Container.Bind<ISaveService>().To<SaveService>().AsSingle();
        Container.Bind<IComponentFinder>().To<OptimizedComponentFinder>().AsSingle();
        Container.Bind<INormalizationService>().To<NormalizationService>().AsSingle();
        Container.Bind<ISwipeController>().To<SwipeController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<BalloonPool>().AsSingle();
    }
}