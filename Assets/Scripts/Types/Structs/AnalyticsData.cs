using System;
using UnityEngine;

public readonly struct AnalyticsData
{
    public readonly int TotalMoneyCollected;
    public readonly int TotalCollisions;
    public readonly EnumRoundResult Result;
    public readonly float RoundDuration;

    public readonly float? PlayerStartHeight;
    public readonly float? PlayerEndHeight;

    public AnalyticsData(int totalMoneyCollected, int totalCollisions, EnumRoundResult result, float roundDuration, float? playerStartHeight, float? playerEndHeight)
    {
        TotalMoneyCollected = totalMoneyCollected;
        TotalCollisions = totalCollisions;
        Result = result;
        RoundDuration = roundDuration;
        PlayerStartHeight = playerStartHeight;
        PlayerEndHeight = playerEndHeight;
    }

    public bool Equals(AnalyticsData other)
    {
        return TotalMoneyCollected == other.TotalMoneyCollected &&
               TotalCollisions == other.TotalCollisions &&
               Result == other.Result &&
               UnityEngine.Mathf.Approximately(RoundDuration, other.RoundDuration) &&
               Nullable.Equals(PlayerStartHeight, other.PlayerStartHeight) &&
               Nullable.Equals(PlayerEndHeight, other.PlayerEndHeight);
    }

    public override bool Equals(object obj)
    {
        if (obj is AnalyticsData other)
        {
            return TotalMoneyCollected == other.TotalMoneyCollected &&
                   TotalCollisions == other.TotalCollisions &&
                   Result == other.Result &&
                   Mathf.Approximately(RoundDuration, other.RoundDuration) &&
                   Nullable.Equals(PlayerStartHeight, other.PlayerStartHeight) &&
                   Nullable.Equals(PlayerEndHeight, other.PlayerEndHeight);
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 31 + TotalMoneyCollected.GetHashCode();
            hash = hash * 31 + TotalCollisions.GetHashCode();
            hash = hash * 31 + Result.GetHashCode();
            hash = hash * 31 + RoundDuration.GetHashCode();
            hash = hash * 31 + (PlayerStartHeight.HasValue ? PlayerStartHeight.Value.GetHashCode() : 0);
            hash = hash * 31 + (PlayerEndHeight.HasValue ? PlayerEndHeight.Value.GetHashCode() : 0);

            return hash;
        }
    }

    public static bool operator ==(AnalyticsData left, AnalyticsData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnalyticsData left, AnalyticsData right)
    {
        return !(left == right);
    }
}
