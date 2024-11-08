using UnityEngine;

public class BaseModule : MonoBehaviour
{

    protected void SnapToPlayer(Transform player)
    {
        transform.parent = player;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

}

