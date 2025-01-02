using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Zenject;

public class BackActionHandler : MonoBehaviour
{
    [SerializeField] UnityEvent OnBackPressed;

    InputAction _backAction;

    [Inject]
    public void Construct(Controls controls)
    {
        _backAction = controls.Player.Back;
    }

    private void OnEnable()
    {
        if (_backAction != null)
        {
            _backAction.performed += HandleBackAction;
            _backAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (_backAction != null)
        {
            _backAction.performed -= HandleBackAction;
            _backAction.Disable();
        }
    }

    private void HandleBackAction(InputAction.CallbackContext context)
    {
        OnBackPressed?.Invoke();
    }
}
