using UnityEngine;
public class IndependentReward : RewardForController
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    static readonly Color _defaultColor = Color.white;
    static readonly Color _collectedColor = new Color(1f, 1f, 1f, 0.1f);

    Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public override void SetNewPosition(Vector2 position)
    {
        base.SetNewPosition(position);

        _spriteRenderer.color = _defaultColor;
        _collider.enabled = true;
    }

    private void OnBecameVisible()
    {
        _spriteRenderer.color = _defaultColor;
        _collider.enabled = true;
    }

    private void OnBecameInvisible()
    {
        _spriteRenderer.color = _defaultColor;
        _collider.enabled = true;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _spriteRenderer.color = _collectedColor;
            _collider.enabled = false;

            IncreasePoints();
        }
    }

}
