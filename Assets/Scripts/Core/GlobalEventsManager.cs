using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class GlobalEventsManager
{
    readonly ITransitionAnimator _transitionAnimator;
    readonly Dictionary<int, TransitionTask> _transitionTasks = new();
    int _nextTaskId = 0;

    event Action RoundStarted;
    event Action RoundEnded;
    event Action WarpStarted;

    bool _useWarpEngineFlag;

    public bool IsTransitioning { get; private set; }

    public GlobalEventsManager(ITransitionAnimator transitionAnimator)
    {
        _transitionAnimator = transitionAnimator ?? throw new ArgumentNullException(nameof(transitionAnimator));
    }

    public async UniTask PerformTransitionAsync()
    {
        IsTransitioning = true;

        await _transitionAnimator.PlayStartTransitionAsync();

        var backgroundTasks = _transitionTasks.Values
         .Where(t => t.RunOnThreadPool)
         .Select(t => UniTask.RunOnThreadPool(t.Task))
         .ToList();

        var mainThreadTasks = _transitionTasks.Values
            .Where(t => !t.RunOnThreadPool)
            .Select(t => t.Task())
            .ToList();

        var allTasks = backgroundTasks.Concat(mainThreadTasks).ToList();

        if (allTasks.Any())
        {
            await UniTask.WhenAll(allTasks);
        }

        await _transitionAnimator.PlayEndTransitionAsync();

        IsTransitioning = false;
    }

    public void StartWarpMoving()
    {
        _useWarpEngineFlag = true;
        OnWarpStart();
        PerformTransitionAsync().Forget();
    }

    public void WarpEnd()
    {
        _useWarpEngineFlag = false;

        OnRoundStart();
        PerformTransitionAsync().Forget();
    }

    public void PlayerLeftThePlatform()
    {
        if (_useWarpEngineFlag) return;

        OnRoundStart();
        PerformTransitionAsync().Forget();
    }

    public void PlayerDied()
    {
        _useWarpEngineFlag = false;

        OnRoundEnd();
        PerformTransitionAsync().Forget();
    }

    private void OnWarpStart() => SafeInvoke(WarpStarted, nameof(WarpStarted));
    private void OnRoundStart() => SafeInvoke(RoundStarted, nameof(RoundStarted));
    private void OnRoundEnd() => SafeInvoke(RoundEnded, nameof(RoundEnded));

    private void SafeInvoke(Action eventAction, string eventName)
    {
        if (eventAction == null) return;

        foreach (var handler in eventAction.GetInvocationList())
        {
            try
            {
                ((Action)handler)?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in {eventName} handler: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    public void SubscribeToRoundStarted(Action action) => RoundStarted += action;
    public void UnsubscribeFromRoundStarted(Action action) => RoundStarted -= action;

    public void SubscribeToRoundEnded(Action action) => RoundEnded += action;
    public void UnsubscribeFromRoundEnded(Action action) => RoundEnded -= action;

    public void SubscribeToWarpStarted(Action action) => WarpStarted += action;
    public void UnsubscribeFromWarpStarted(Action action) => WarpStarted -= action;

    public int AddTransitionTask(Func<UniTask> task, bool runOnThreadPool = false)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var transitionTask = new TransitionTask(task, runOnThreadPool);
        _transitionTasks[_nextTaskId] = transitionTask;

        return _nextTaskId++;
    }

    public void RemoveTransitionTask(int taskId)
    {
        if (!_transitionTasks.ContainsKey(taskId))
            throw new ArgumentOutOfRangeException($"{nameof(taskId)}: {taskId}");

        _transitionTasks.Remove(taskId);
    }

    private readonly struct TransitionTask
    {
        public readonly Func<UniTask> Task;
        public readonly bool RunOnThreadPool;

        public TransitionTask(Func<UniTask> task, bool runOnThreadPool)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            RunOnThreadPool = runOnThreadPool;
        }
    }
}

