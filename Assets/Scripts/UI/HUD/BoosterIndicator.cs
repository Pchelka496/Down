using UnityEngine;

public class BoosterIndicator : MonoBehaviour
{
    [SerializeField] GameObject _ready;
    [SerializeField] GameObject _unready;

    public void SetReadyStatus()
    {
        _ready.SetActive(true);
        _unready.SetActive(false);
    }

    public void SetUnreadyStatus()
    {
        _ready.SetActive(false);
        _unready.SetActive(true);
    }

}
