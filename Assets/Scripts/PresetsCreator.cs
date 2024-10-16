using UnityEngine;

[ExecuteInEditMode]
public class PresetsCreator : MonoBehaviour
{
    [SerializeField] public RewardControllerConfig _rewardControllerConfig;
    [SerializeField] public RewardControllerConfig.RewardPosition _rewardPosition;

#if UNITY_EDITOR

    private void Reset()
    {
        _rewardControllerConfig = FindRewardConfig();
    }

    private RewardControllerConfig FindRewardConfig()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RewardControllerConfig");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<RewardControllerConfig>(path);
        }
        return null;
    }

    public void AddRewardPosition()
    {
        if (_rewardControllerConfig != null && _rewardPosition != null)
        {
            _rewardControllerConfig.AddLastRewardPreset(_rewardPosition);
        }
    }

    public void RemoveLastRewardPosition()
    {
        if (_rewardControllerConfig != null)
        {
            _rewardControllerConfig.DeleteLastPreset();
        }
    }
#endif
}