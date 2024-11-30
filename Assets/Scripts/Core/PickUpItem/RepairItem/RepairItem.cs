using UnityEngine;
using Zenject;

public class RepairItem : MonoBehaviour
{
    const string APPLY_KIT_ANIMATION_STATE_NAME = "ApplyKit";

    [SerializeField] Collider2D _collider;
    [SerializeField] Animator _animator;
    [SerializeField] SpriteRenderer _spriteRender;
    [SerializeField] Material _defaultMaterial;
    [SerializeField] Material _flashMaterial;
    [SerializeField] SoundPlayerRandomPitch _soundPlayer;
    HealthModule _healthModule;
    GameObject _player;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(PlayerController player, AudioSourcePool audioSourcePool)
    {
        _player = player.gameObject;
        _healthModule = player.HealthModule;
        _soundPlayer.Initialize(audioSourcePool);
    }

    public void Relocate(Vector2 newPosition)
    {
        transform.position = newPosition;
        SetDefaultMaterial();
        gameObject.SetActive(true);
        _collider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player)
        {
            ApplyRepairKit();
            ApplyEffect();
        }
    }

    private void ApplyRepairKit()
    {
        _healthModule.ApplyRepairKit();
        _collider.enabled = false;
    }

    private void ApplyEffect()
    {
        _animator.Play(APPLY_KIT_ANIMATION_STATE_NAME);
        _soundPlayer.PlayNextSound();
    }

    //Animation event
    public void SetFlashMaterial() => _spriteRender.material = _flashMaterial;
    public void SetDefaultMaterial() => _spriteRender.material = _defaultMaterial;
    public void ApplyKitAnimationEnd()
    {
        gameObject.SetActive(false);
        _spriteRender.transform.localScale = Vector3.one;
    }

}
