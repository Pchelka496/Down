using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundControllerConfig", menuName = "Scriptable Objects/BackgroundControllerConfig")]
public class BackgroundControllerConfig : ScriptableObject
{
    [SerializeField] Gradient _backgroundGradient;

    public Gradient BackgroundGradient { get => _backgroundGradient; }

}
