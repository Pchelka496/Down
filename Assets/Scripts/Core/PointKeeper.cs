using UnityEngine;

public class PointKeeper
{
    PointKeeperConfig _config;

    public void Initialize(PointKeeperConfig config)
    {
        _config = config;
    }

    public int GetPoint() => _config.Points;

    public void IncreesePoint(int value)
    {

    }

    public void DecreasePoint(int value)
    {

    }

}
