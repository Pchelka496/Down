using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

public class RewardForController : MonoBehaviour
{
    [SerializeField] int _rewardAmount = 1;
    protected static CharacterController _player;
    protected static RewardManager _rewardManager;
    protected static RewardCounter _rewardCounter;

    [Inject]
    private void Construct(CharacterController player, RewardManager rewardManager, RewardCounter rewardCounter)
    {
        _player = player;
        _rewardManager = rewardManager;
        _rewardCounter = rewardCounter;
    }

    public virtual void SetNewPosition(Vector2 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            gameObject.SetActive(false);
            IncreasePoints();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void IncreasePoints()
    {
        _rewardManager.IncreasePoints(_rewardAmount);
        _rewardCounter.IncreasePointsPerRound(_rewardAmount);
    }

}
