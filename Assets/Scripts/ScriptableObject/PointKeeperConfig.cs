using UnityEngine;

[CreateAssetMenu(fileName = "PointKeeperConfig", menuName = "Scriptable Objects/PointKeeperConfig")]
public class PointKeeperConfig : ScriptableObject
{
    [SerializeField] int _points;

    public int Points { get => _points; set => _points = value; }

}
