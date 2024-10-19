using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField][Range(0f, 3f)] private float _minPitch = 0.8f;
    [SerializeField][Range(0f, 3f)] private float _maxPitch = 1.2f;
    [SerializeField][Range(0f, 1f)] private float _volume = 0.7f;
    [SerializeField] private AudioClip _pickUpRewardAudio;
    [SerializeField] private TextMeshProUGUI _text;

    [SerializeField][Range(0.1f, 10f)] private float _pitchResetDelay = 3f;  // Время до сброса питча
    [SerializeField] private int _maxCoinsForMaxPitch = 5;  // Количество монет до максимального питча

    private AudioSourcePool _audioSourcePool;
    private int _points;
    private int _coinsCollectedInARow = 0;  // Монеты подряд
    private float _currentPitch;  // Текущий питч

    private CancellationTokenSource _resetPitchTokenSource;

    [Inject]
    private void Construct(LevelManager levelManager, AudioSourcePool audioSource)
    {
        _audioSourcePool = audioSource;
        gameObject.SetActive(false);
        levelManager.SubscribeToRoundStart(RoundStart);
        ResetPoints();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        ResetPoints();
        gameObject.SetActive(true);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += increaseValue;

        _coinsCollectedInARow++;
        UpdatePitch();

        _audioSourcePool.PlaySound(_pickUpRewardAudio, _volume, _currentPitch);
        UpdateText();

        ResetPitchAfterDelay().Forget();  // Запускаем сброс питча
    }

    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void ResetPoints()
    {
        _points = 0;
        _coinsCollectedInARow = 0;
        _currentPitch = _minPitch;
        UpdateText();
    }

    private void UpdatePitch()
    {
        float pitchProgress = Mathf.Clamp01((float)_coinsCollectedInARow / _maxCoinsForMaxPitch);
        _currentPitch = Mathf.Lerp(_minPitch, _maxPitch, pitchProgress);
    }

    private async UniTaskVoid ResetPitchAfterDelay()
    {
        _resetPitchTokenSource?.Cancel();
        _resetPitchTokenSource = new CancellationTokenSource();

        await UniTask.Delay(TimeSpan.FromSeconds(_pitchResetDelay), cancellationToken: _resetPitchTokenSource.Token);
        _coinsCollectedInARow = 0;
    }

}

