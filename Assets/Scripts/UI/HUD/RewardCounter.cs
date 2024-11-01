using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Zenject;

public class RewardCounter : MonoBehaviour
{
    [SerializeField] SoundPlayerIncreasePitch _soundPlayer;
    [SerializeField] TextMeshProUGUI _text;

    int _points;

    [Inject]
    private void Construct(LevelManager levelManager, AudioSourcePool audioSource)
    {
        _soundPlayer.Initialize(audioSource);
    }

    private void OnEnable()
    {
        _points = 0;
        UpdateText();
    }

    public void IncreasePointsPerRound(int increaseValue)
    {
        _points += increaseValue;

        _soundPlayer.PlaySound();

        UpdateText();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateText()
    {
        _text.text = _points.ToString();
    }

    private void OnDestroy()
    {
        _soundPlayer.Dispose();
    }

}

