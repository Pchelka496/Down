using UnityEngine;

public class PlatformPiece : MonoBehaviour
{
    [SerializeField] Collider2D _collider;
    [SerializeField] Vector2 _size;

    public Vector2 Size { get => _size; set => _size = value; }

}
