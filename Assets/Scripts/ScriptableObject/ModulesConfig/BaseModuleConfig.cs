using UnityEngine;

public abstract class BaseModuleConfig : ScriptableObject
{
    [Header("Array index is a level")]
    [SerializeField][Range(0, 100)] protected int _currentLevel = 0;
    [SerializeField] protected int[] _levelCost = new int[0];

    public abstract void SetLevel(int level);
    public virtual int MaxLevel() => _levelCost.Length - 1;
    public virtual bool ActivityCheck() => _currentLevel > 0;
    public int[] GetLevelCost() => _levelCost;
    public int GetCurrentLevelCost()
    {
        if (_currentLevel < _levelCost.Length)
        {
            return _levelCost[_currentLevel];
        }
        else
        {
            return default;
        }
    }

    public int GetCurrentLevel() => _currentLevel;
    public abstract int GetMaxLevel();
    public abstract bool SetLevelCheck(int level);

}
