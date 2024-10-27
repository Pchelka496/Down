using UnityEngine;
using UnityEngine.Events;
using Zenject;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : MonoBehaviour
{
    [Header("Called when the player enters the trigger area")]
    [SerializeField] UnityEvent onPlayerEnter;
    CharacterController _player;

    [Inject]
    private void Construct(CharacterController player)
    {
        _player = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            onPlayerEnter?.Invoke();
        }
    }

}
