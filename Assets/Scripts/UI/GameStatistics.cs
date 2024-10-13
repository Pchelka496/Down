using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameStatistics : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _fpsTextMeshPro;
    [SerializeField] TextMeshProUGUI _averageFpsTextMeshPro;
    [SerializeField] int _averageFpsUpdateInterval = 1;

    float _deltaTime;
    float _totalDeltaTime;
    int _frameCount;

    private void Start()
    {
        _ = UpdateAverageFps();
    }

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _totalDeltaTime += Time.unscaledDeltaTime;
        _frameCount++;

        float fps = 1.0f / _deltaTime;
        _fpsTextMeshPro.text = string.Format("FPS: {0:0.}", fps);
    }

    private async UniTask UpdateAverageFps()
    {
        while (true)
        {
            float averageFps = _frameCount / _totalDeltaTime;
            _averageFpsTextMeshPro.text = string.Format("Average FPS: {0:0.}", averageFps);

            _frameCount = 0;
            _totalDeltaTime = 0;

            await UniTask.WaitForSeconds(_averageFpsUpdateInterval);
        }
    }

}