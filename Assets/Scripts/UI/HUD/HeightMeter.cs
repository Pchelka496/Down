using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class HeightMeter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _currentHeight;
    Transform _playerTransform;

    [Inject]
    private void Construct(CharacterController player)
    {
        _playerTransform = player.transform;
    }

    private void Start()
    {
        _ = UpdateAverageFps();
    }

    private async UniTask UpdateAverageFps()
    {
        while (true)
        {
            _currentHeight.text = _playerTransform.position.y.ToString("F0") + " m";
            await UniTask.WaitForSeconds(0.2f);
        }
    }

}
