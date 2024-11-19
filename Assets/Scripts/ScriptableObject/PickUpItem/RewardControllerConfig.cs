using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "RewardControllerConfig", menuName = "Scriptable Objects/RewardControllerConfig")]
public class RewardControllerConfig : ScriptableObject
{
    [SerializeField] AssetReference _rewardPrefabReference;
    [SerializeField] RewardPosition[] _rewardPresets = new RewardPosition[1];

    public AssetReference RewardPrefabReference { get => _rewardPrefabReference; set => _rewardPrefabReference = value; }

    public RewardPosition[] GetRewardPresets() => _rewardPresets;

    public int GetMaxRewardCount()
    {
        int maxRewardCount = 0;

        foreach (var rewardPreset in GetRewardPresets())
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

        public RewardPosition(Vector2[] rewardPositions, Vector2 size)
        {
            _rewardPositions = rewardPositions;
            _size = size;
        }

        public Vector2[] RewardPositions => _rewardPositions;

        public Vector2 Size => _size;

    }

#if UNITY_EDITOR
    public RewardPosition[] SetRewardPresets(RewardPosition[] value) => _rewardPresets = value;

    public void AddLastRewardPreset(RewardPosition rewardPosition)
    {
        var rewardPresets = GetRewardPresets();
        var newRewardPresets = new RewardPosition[rewardPresets.Length + 1];

        for (int i = 0; i < rewardPresets.Length; i++)
        {
            newRewardPresets[i] = rewardPresets[i];
        }

        var rewardPositionCopy = new RewardPosition((Vector2[])rewardPosition.RewardPositions.Clone(), rewardPosition.Size);
        newRewardPresets[rewardPresets.Length] = rewardPositionCopy;

        SetRewardPresets(newRewardPresets);
    }

    public void DeleteLastPreset()
    {
        var rewardPresets = GetRewardPresets();

        if (rewardPresets == null || rewardPresets.Length == 0)
        {
            Debug.LogWarning("Array is empty or not initialized");
            return;
        }

        var newRewardPresets = new RewardPosition[rewardPresets.Length - 1];

        for (int i = 0; i < rewardPresets.Length - 1; i++)
        {
            newRewardPresets[i] = rewardPresets[i];
        }

        SetRewardPresets(newRewardPresets);
    }

#endif

}
