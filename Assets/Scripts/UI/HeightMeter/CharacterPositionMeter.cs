using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class CharacterPositionMeter : MonoBehaviour
{
    public static float YPosition { get; private set; }
    public static float XPosition { get; private set; }

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
        var position = _playerTransform.position;

        while (true)
        {
            position = _playerTransform.position;

            YPosition = position.y;
            XPosition = position.x;

            _currentHeight.text = YPosition.ToString("F0") + " m";

            await UniTask.WaitForSeconds(0.2f);
        }
    }

}
