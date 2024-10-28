using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Pair<F, S>
{
    [field: SerializeField] public F First { get; set; }
    [field: SerializeField] public S Second { get; set; }

    public Pair(Pair<F, S> copier)
    {
        copy(copier);
    }

    public Pair(F First, S Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public void set(F First, S Second)
    {
        this.First = First;
        this.Second = Second;
    }

    public void copy(Pair<F, S> copier)
    {
        this.First = copier.First;
        this.Second = copier.Second;
    }

    public bool equals(Pair<F, S> compare)
    {
        return this.First.Equals(compare.First) && this.Second.Equals(compare.Second);
    }

    public bool equals(object first, object second)
    {
        return First.Equals(first) && Second.Equals(second);
    }

    public override bool Equals(object obj)
    {
        Debug.LogWarning("Did you mean \"equals\" instead of \"Equals\" in the pair?");
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool InList(List<Pair<F,S>> list)
    {
        foreach(var pair in list.ToList()) if(pair.equals(this)) return true;
        return false;
    }

    public string DetailedToString()
    {
        return "Pair<" + typeof(F) + ", " + typeof(S) + "> = (" + First + ", " + Second + ")";
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", First, Second);
    }
}