using Cysharp.Threading.Tasks;
using System;
using Zenject;

public class NormalizationService : INormalizationService
{
    [Inject] private IComponentFinder _componentFinder;

    private bool _inProgress;

    public async UniTask Normalize(IGrid grid)
    {
        if (_inProgress)
            return;

        _inProgress = true;

        bool changed;
        do
        {
            changed = false;
            try
            {
                await grid.DropDown();
                var components = _componentFinder.FindComponents(grid);
                if (components.Count > 0)
                {
                    changed = true;
                    await grid.DestroyComponents(components);
                }
            }
            catch(OperationCanceledException)
            {
                break;
            }
        }
        while (changed);

        _inProgress = false;
    }
}