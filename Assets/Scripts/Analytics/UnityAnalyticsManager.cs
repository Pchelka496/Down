using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;

public class UnityAnalyticsManager : IAnalyticsManager
{
    public UnityAnalyticsManager()
    {
        InitializeAnalytics().Forget();
    }

    private async UniTask InitializeAnalytics()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection(); 
    }

    public void SendAnalyticsEvent(AnalyticsData data)
    {
        CustomEvent @event = new("RoundReport")
        {
            { "TotalMoneyCollected", data.TotalMoneyCollected  },
            { "TotalCollisions", data.TotalCollisions  },
            { "RoundResult", data.Result.ToString() },
            { "RoundDuration", data.RoundDuration  },
            { "PlayerStartHeight", data.PlayerStartHeight },
            { "PlayerEndHeight", data.PlayerEndHeight }
        };

        AnalyticsService.Instance.RecordEvent(@event);
        AnalyticsService.Instance.Flush();
    }
}
