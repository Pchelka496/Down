using Cysharp.Threading.Tasks;
using ScriptableObject;
using UnityEngine;
using Zenject;

public class PerformanceManager
{
    event System.Action DisposeEvents;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        var taskId = globalEventsManager.AddTransitionTask(CleaningAndPlayerTransition, false);
        DisposeEvents += () => globalEventsManager?.RemoveTransitionTask(taskId);
    }

    public void Initialize(int targetFrameRate)
    {
        Application.targetFrameRate = targetFrameRate;
    }

    private UniTask CleaningAndPlayerTransition()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        return UniTask.CompletedTask;
    }

    public void Dispose()
    {
        DisposeEvents?.Invoke();
    }
}
