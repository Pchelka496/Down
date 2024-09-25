using UnityEngine;

[CreateAssetMenu(fileName = "LevelManagerConfig", menuName = "Scriptable Objects/LevelManagerConfig")]
public class LevelManagerConfig : ScriptableObject//one per project
{
    [SerializeField] string _mapControllerConfigAddress;
    [SerializeField] string _backgroundControllerConfigAddress;

    public void ChangeLevel(string mapConfigAddress, string backgroundConfigAddress)
    {

    }

}
