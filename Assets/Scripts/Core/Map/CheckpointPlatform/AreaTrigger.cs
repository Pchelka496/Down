using Creatures.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : MonoBehaviour
{
    [FormerlySerializedAs("onPlayerEnter")]
    [Header("Called when the player enters the trigger area")]
    [SerializeField] UnityEvent _onPlayerEnter;
    PlayerController _player;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(PlayerController player)
    {
        _player = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _onPlayerEnter?.Invoke();
        }
    }

}
