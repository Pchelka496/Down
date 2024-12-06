using UnityEngine;

namespace ScriptableObject
{
    [CreateAssetMenu(fileName = "DisplayControllerConfig", menuName = "Scriptable Objects/DisplayController")]
    public class DisplayControllerConfig : UnityEngine.ScriptableObject
    {
        [Header("StartText+ value +EndText")]
        [SerializeField] string _startPointsInformation;
        [SerializeField] string _endPointsInformation;

        [SerializeField] string _startPointsLack;
        [SerializeField] string _endPointsLack;

        [SerializeField] string _setClimbingMode;
        [SerializeField] string _setDescendingMode;

        public string StartPointsInformation { get => _startPointsInformation; set => _startPointsInformation = value; }
        public string EndPointsInformation { get => _endPointsInformation; set => _endPointsInformation = value; }
        public string StartPointsLack { get => _startPointsLack; set => _startPointsLack = value; }
        public string EndPointsLack { get => _endPointsLack; set => _endPointsLack = value; }
        public string SetClimbingMode => _setClimbingMode;
        public string SetDescendingMode => _setDescendingMode;
    }
}
