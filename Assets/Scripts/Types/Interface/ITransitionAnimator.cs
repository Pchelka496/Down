using Cysharp.Threading.Tasks;

public interface ITransitionAnimator
{
    UniTask PlayStartTransitionAsync();
    UniTask PlayEndTransitionAsync();
}