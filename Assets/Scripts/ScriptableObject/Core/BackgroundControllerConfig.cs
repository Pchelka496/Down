using UnityEngine;

namespace ScriptableObject
{
    [CreateAssetMenu(fileName = "BackgroundControllerConfig", menuName = "Scriptable Objects/BackgroundControllerConfig")]
    public class BackgroundControllerConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] Gradient _backgroundGradient;

        public Gradient BackgroundGradient => _backgroundGradient;
    }
}
