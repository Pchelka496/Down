using UnityEngine;

public class EnemyVisualPart : MonoBehaviour
{
    Transform _enemyCoreTransform;

    public void Initialize(EnemyCore enemyCore)
    {
        _enemyCoreTransform = enemyCore.transform;
    }

    public int IndexInFactory { get; set; }
    public int IndexInManager { get; set; }

}
