using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardControllerConfig", menuName = "Scriptable Objects/RewardControllerConfig")]
public class RewardControllerConfig : ScriptableObject
{
    [SerializeField] string[] _rewardPrefabAddress;
    [SerializeField] RewardPosition[] _rewardPresets = new RewardPosition[1];

    public string[] RewardPrefabAddress { get => _rewardPrefabAddress; set => _rewardPrefabAddress = value; }

    [System.Serializable]
    public record RewardPosition
    {
        [SerializeField] Vector2[] _rewardPositions;

        public Vector2[] RewardPositions { get => _rewardPositions; set => _rewardPositions = value; }
    }

#if UNITY_EDITOR

    public void AddLastRewardPreset(RewardPosition rewardPosition)
    {
        var newRewardPresets = new RewardPosition[_rewardPresets.Length + 1];

        for (int i = 0; i < _rewardPresets.Length; i++)
        {
            newRewardPresets[i] = _rewardPresets[i];
        }

        newRewardPresets[_rewardPresets.Length] = rewardPosition;

        _rewardPresets = newRewardPresets;
    }

    public void DeleteLastPreset()
    {
        if (_rewardPresets == null || _rewardPresets.Length == 0)
        {
            Debug.LogWarning("Массив пуст или не инициализирован.");
            return;
        }

        var newRewardPresets = new RewardPosition[_rewardPresets.Length - 1];

        for (int i = 0; i < _rewardPresets.Length - 1; i++)
        {
            newRewardPresets[i] = _rewardPresets[i];
        }

        _rewardPresets = newRewardPresets;
    }

#endif

}
