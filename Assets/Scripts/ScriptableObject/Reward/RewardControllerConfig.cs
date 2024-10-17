using UnityEngine;

[CreateAssetMenu(fileName = "RewardControllerConfig", menuName = "Scriptable Objects/RewardControllerConfig")]
public class RewardControllerConfig : ScriptableObject
{
    [SerializeField] string _rewardPrefabAddress;
    [SerializeField] RewardPosition[] _rewardPresets = new RewardPosition[1];

    public string RewardPrefabAddress { get => _rewardPrefabAddress; set => _rewardPrefabAddress = value; }
    public RewardPosition[] RewardPresets { get => _rewardPresets; set => _rewardPresets = value; }

    public int GetMaxRewardCount()
    {
        int maxRewardCount = 0;

        foreach (var rewardPreset in RewardPresets)
        {
            if (rewardPreset.RewardPositions != null)
            {
                int currentCount = rewardPreset.RewardPositions.Length;
                if (currentCount > maxRewardCount)
                {
                    maxRewardCount = currentCount;
                }
            }
        }

        return maxRewardCount;
    }

    [System.Serializable]
    public record RewardPosition
    {
        [SerializeField] Vector2[] _rewardPositions;
        [SerializeField] Vector2 _size;

        public Vector2[] RewardPositions { get => _rewardPositions; set => _rewardPositions = value; }
        public Vector2 Size { get => _size; set => _size = value; }
    }

#if UNITY_EDITOR

    public void AddLastRewardPreset(RewardPosition rewardPosition)
    {
        var newRewardPresets = new RewardPosition[RewardPresets.Length + 1];

        for (int i = 0; i < RewardPresets.Length; i++)
        {
            newRewardPresets[i] = RewardPresets[i];
        }

        newRewardPresets[RewardPresets.Length] = rewardPosition;

        RewardPresets = newRewardPresets;
    }

    public void DeleteLastPreset()
    {
        if (RewardPresets == null || RewardPresets.Length == 0)
        {
            Debug.LogWarning("Массив пуст или не инициализирован.");
            return;
        }

        var newRewardPresets = new RewardPosition[RewardPresets.Length - 1];

        for (int i = 0; i < RewardPresets.Length - 1; i++)
        {
            newRewardPresets[i] = RewardPresets[i];
        }

        RewardPresets = newRewardPresets;
    }

#endif

}
