using System;

[Serializable]
public class SerializableDictionaryEntry<TKey, TValue>
{
    public TKey key;
    public TValue value;

    public SerializableDictionaryEntry(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }

    public Type keyType()
    {
        return typeof(TKey);
    }

    public Type valueType()
    {
        return typeof(TValue);
    }
}
