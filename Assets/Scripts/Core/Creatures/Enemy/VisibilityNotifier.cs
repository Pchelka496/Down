using UnityEngine;

public class VisibilityNotifier : MonoBehaviour
{
    [SerializeField] EnemyChallengeUpdateComponent _challengeComponent;
    [SerializeField] int _visibilityIndex;

#if UNITY_EDITOR
    public int VisibilityIndex { get => _visibilityIndex; set => _visibilityIndex = value; }
    public EnemyChallengeUpdateComponent ChallengeComponent { get => _challengeComponent; set => _challengeComponent = value; }

#endif

    private void OnBecameInvisible()
    {
        _challengeComponent.UpdateVisibilityStatus(_visibilityIndex, false);
    }

    private void OnBecameVisible()
    {
        _challengeComponent.UpdateVisibilityStatus(_visibilityIndex, true);
    }
}

