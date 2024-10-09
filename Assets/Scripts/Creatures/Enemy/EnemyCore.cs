using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    GameObject _visualPart;

    public void Initialize(GameObject visualPart)
    {
        if (_visualPart != null)
        {
            Destroy(_visualPart);
        }

        _visualPart = visualPart;
        _visualPart.transform.parent = transform;
        _visualPart.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

}
