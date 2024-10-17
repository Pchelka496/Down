using UnityEngine;
using Zenject;

public class Reward : MonoBehaviour
{
    static CharacterController _player;
    static RewardManager _rewardManager;
    static RewardCounter _rewardCounter;

    [Inject]
    private void Construct(CharacterController player, RewardManager rewardManager, RewardCounter rewardCounter)
    {
        _player = player;
        _rewardManager = rewardManager;
        _rewardCounter = rewardCounter;
    }

    public void SetNewPosition(Vector2 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            gameObject.SetActive(false);
            _rewardManager.IncreasePoints(1);
            _rewardCounter.IncreasePointsPerRound(1);
        }

    }

}
