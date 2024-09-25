using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundControllerConfig", menuName = "Scriptable Objects/BackgroundControllerConfig")]
public class BackgroundControllerConfig : ScriptableObject
{
    [SerializeField] string _backgroundPrefabAddress;

}
