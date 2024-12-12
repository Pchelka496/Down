using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "PostProcessingControllerConfig", menuName = "Scriptable Objects/PostProcessingControllerConfig")]
public class PostProcessingControllerConfig : UnityEngine.ScriptableObject
{
    [SerializeField] VolumeProfileData _lobby;
    [SerializeField] VolumeProfileData _gameplay;
    [SerializeField] VolumeProfileData _warp;
    [SerializeField] VolumeProfileData _playerTakeImpact;

    [SerializeField] float _takeImpactVolumeDuration;

    public float TakeImpactVolumeDuration => _takeImpactVolumeDuration;

    public VolumeProfileData Lobby => _lobby;
    public VolumeProfileData Gameplay => _gameplay;
    public VolumeProfileData Warp => _warp;
    public VolumeProfileData PlayerTakeImpact => _playerTakeImpact;


    [System.Serializable]
    public record VolumeProfileData()
    {
        [SerializeField] VolumeProfile _volumeProfile;
        [SerializeField] float _transitionToDuration;
        [SerializeField] float _transitionFromDuration;

        public VolumeProfile VolumeProfile => _volumeProfile;
        public float TransitionToDuration => _transitionToDuration;
        public float TransitionFromDuration => _transitionFromDuration;
    }
}
