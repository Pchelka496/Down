using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class GlobalEventsManager : IRoundResultTracker
{
    readonly ITransitionAnimator _transitionAnimator;
    readonly Dictionary<int, TransitionTask> _transitionTasks = new();
    int _nextTaskId = 0;

    EnumRoundResult _roundResult;

    event Action RoundStarted;
    event Action RoundEnded;
    event Action FastTravelStarted;
    event Action<EnumRoundResult> OnRoundResultChanged;

    bool _useWarpEngineFlag;

    public bool IsTransitioning { get; private set; }
    public EnumRoundResult RoundResult
    {
        set
        {
            _roundResult = value;
            OnRoundResultChanged?.Invoke(_roundResult);
        }
    }

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
        RoundResult = EnumRoundResult.Defeat;

        OnRoundEnd();
        PerformTransitionAsync().Forget();
    }

    public void PlayerReachedSurface()
    {
        _useWarpEngineFlag = false;
        RoundResult = EnumRoundResult.Victory;

        OnRoundEnd();
        PerformTransitionAsync().Forget();
    }

    private void OnWarpStart() => SafeInvoke(FastTravelStarted, nameof(FastTravelStarted));
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

    public void SubscribeToFastTravelStarted(Action action) => FastTravelStarted += action;
    public void UnsubscribeFromFastTravelStarted(Action action) => FastTravelStarted -= action;

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

    EnumRoundResult IRoundResultTracker.GetRoundResult() => _roundResult;
    void IRoundResultTracker.SubscribeToRoundResultChanged(Action<EnumRoundResult> callback) => OnRoundResultChanged += callback;
    void IRoundResultTracker.UnsubscribeFromRoundResultChanged(Action<EnumRoundResult> callback) => OnRoundResultChanged -= callback;

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
