using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

public class Reward : MonoBehaviour
{
    [SerializeField] int _rewardAmount = 1;
    static CharacterController _player;
    static RewardCounter _rewardCounter;

    public static void Initialize(CharacterController player, RewardCounter rewardCounter)
    {
        _player = player;
        _rewardCounter = rewardCounter;
    }

    private void Start()
    {
        gameObject.layer = PickUpItemManager.REWARD_LAYER_INDEX;
    }

    public virtual void SetNewPosition(Vector2 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);
        IncreasePoints();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void IncreasePoints()
    {
        _rewardCounter.IncreasePointsPerRound(_rewardAmount);
    }

}
