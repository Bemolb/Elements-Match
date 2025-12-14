using Cysharp.Threading.Tasks;
public interface INormalizationService
{
    UniTask Normalize(IGrid grid);
}
