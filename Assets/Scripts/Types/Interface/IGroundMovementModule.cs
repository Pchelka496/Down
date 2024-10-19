using Cysharp.Threading.Tasks;
using System.Threading;

public interface IGroundMovementModule
{
    public UniTask GroundMovement(CancellationTokenSource tokenSource);

}
