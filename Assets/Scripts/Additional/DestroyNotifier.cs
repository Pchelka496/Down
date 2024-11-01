using System;
using UnityEngine;

public class DestroyNotifier : MonoBehaviour
{
    event System.Action _onDestroyed;

    public void SubscribeToOnDestroyed(Action action) => _onDestroyed += action;

    private void OnDestroy()
    {
        _onDestroyed?.Invoke();
        _onDestroyed = null;
    }

}
