using UnityEngine;

[System.Serializable]
public struct CustomKeyValuePair<TKey, TValue>
{
    public CustomKeyValuePair(TKey key, TValue value)
    {
        _key = key;
        _value = value;
    }

    [SerializeField] TKey _key;
    [SerializeField] TValue _value;

    public readonly TKey Key => _key;
    public readonly TValue Value => _value;

    public readonly void Deconstruct(out TKey key, out TValue value)
    {
        key = Key;
        value = Value;
    }

    public override readonly string ToString()
    {
        return $"Key: {Key}, Value: {Value}";
    }

}
