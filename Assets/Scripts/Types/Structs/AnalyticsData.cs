public readonly struct AnalyticsData
{
    public readonly int TotalMoneyCollected;
    public readonly int TotalCollisions;
    public readonly RoundResult Result;
    public readonly float RoundDuration;

    public readonly float? PlayerStartHeight;
    public readonly float? PlayerEndHeight;

    public AnalyticsData(int totalMoneyCollected, int totalCollisions, RoundResult result, float roundDuration, float? playerStartHeight, float? playerEndHeight)
    {
        TotalMoneyCollected = totalMoneyCollected;
        TotalCollisions = totalCollisions;
        Result = result;
        RoundDuration = roundDuration;
        PlayerStartHeight = playerStartHeight;
        PlayerEndHeight = playerEndHeight;
    }
}
