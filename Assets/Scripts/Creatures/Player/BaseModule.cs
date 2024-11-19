using UnityEngine;

public abstract class BaseModule : MonoBehaviour
{
    [SerializeField] bool _isActiveOnLobbyMode;

    public bool IsActiveOnLobbyMode => _isActiveOnLobbyMode;

    public abstract void EnableModule();
    public abstract void DisableModule();

    protected void SnapToPlayer(Transform player) => SnapToPlayer(player, Vector3.zero, Quaternion.identity);

    protected void SnapToPlayer(Transform player, Vector3 localPosition, Quaternion localRotation)
    {
        transform.parent = player;
        transform.SetLocalPositionAndRotation(localPosition, localRotation);
    }

}

