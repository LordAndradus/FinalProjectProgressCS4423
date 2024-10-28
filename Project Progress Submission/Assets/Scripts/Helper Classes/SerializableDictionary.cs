using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<SerializableDictionaryEntry<TKey, TValue>> elements = new();

    public SerializableDictionary() : base() {}

    //Either use this, or SerializableDictionary<TKey, TValue) something = (SerializableDictionary<TKey, TValue>) new Dictionary<TKey, TValue>(SerializableDictionary copy);
    public SerializableDictionary(Dictionary<TKey, TValue> copy) : base(copy) {}

    public void OnAfterDeserialize()
    {
        this.Clear();

        foreach(var entry in elements) this[entry.key] = entry.value;
    }

    public void OnBeforeSerialize()
    {
        elements.Clear();

        foreach(KeyValuePair<TKey, TValue> entry in this) elements.Add(new SerializableDictionaryEntry<TKey, TValue>(entry.Key, entry.Value));
    }

    public TKey getKey(TValue reference)
    {
        List<KeyValuePair<TKey, TValue>> kvList = new();

        foreach(var entry in this) if(entry.Value.Equals(reference)) kvList.Add(entry);

        if(kvList.Count == 0) throw new Exception("Key does not exist for the value. Passed = " + reference);
        else if(kvList.Count > 1)
        {
            StringBuilder ErrorLog = new();

            ErrorLog.Append("Error: Multiple keys detected. Size = " + kvList.Count);
            ErrorLog.Append("\nList of matching entries---\n");
            foreach(var entry in kvList) ErrorLog.Append("Key = " + entry.Key + " | Value = " + entry.Value + "\n");
            ErrorLog.Append("Sending first key in this list.");
            Debug.Log(ErrorLog.ToString());

            throw new Exception("Too many keys to fetch inside dictionary!");
        }

        return kvList[0].Key;
    }

    public string toString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Serializable Dictionary - <TKey> = ").Append(typeof(TKey)).Append(" <TValue> = ").Append(typeof(TValue)).Append("\n");

        sb.Append("Size of dictionary = ").Append(this.Count).Append("\n\n");
        
        sb.Append("Normal dictionary\n");
        int index = 0;
        foreach(var element in this) sb.Append("Pair ").Append((index++)).Append(" = (Key: [").Append(element.Key).Append("], Value: [").Append(element.Value).Append("])\n");
        if(Count == 0) sb.Append("[Empty]\n");

        sb.Append("\nSerialized list\n");
        index = 0;
        foreach(var element in elements) sb.Append("Pair ").Append((index++)).Append(" = (Key: [").Append(element.key).Append("], Value: [").Append(element.value).Append("])\n");
        if(elements.Count == 0) sb.Append("[Empty]\n");

        sb.Append("\n");

        return sb.ToString();
    }
}
