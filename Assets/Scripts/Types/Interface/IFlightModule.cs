using Cysharp.Threading.Tasks;
using System.Threading;

public interface IFlightModule
{
    public UniTask Fly(CancellationTokenSource tokenSource);

}
