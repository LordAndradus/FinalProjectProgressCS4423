using System;
using UnityEngine;

public class Trait
{
    Unit reference;
    
    public event Action addEffect;
    public event Action removeEffect;
    public event Action triggerEffect;

    public Sprite icon;

    public string UIFriendlyClassName;

    public string ShortDescription = "Learnable slot for a trait!";
    public string FlavorText = "A slot that can be used to learn a trait!";
    public string EffectText = "There is untapped potential to make this unit more effective...";

    public TraitRank rank = TraitRank.Normal;

    protected bool combat = false; //Indicates if it's a status changer, or if it's for combat

    // Start is called before the first frame update
    public Trait()
    {
        UIFriendlyClassName = UtilityClass.UIFriendlyClassName(GetType().Name);
        addEffect += add;
        removeEffect += remove;
    }

    public String getDescription()
    {
        return FlavorText + " " + EffectText;
    }

//Overriden functions========================================================================================================================
    public virtual void add()
    {
        Debug.Log("Don't forget to add an effect!");
    }

    public virtual void remove() //For non-combat effects
    {
        Debug.Log("Make sure to actually remove the effect!");
    }

    public virtual String toString()
    {
        return "Name = \"" + GetType().Name + "\", effect = \"" + EffectText + "\", Description = \"" + FlavorText + "\"";
    }

    public enum TraitRank
    {
        Legendary, //Massively boosts a unit, the rarest trait in the game
        Leader, //Effectively only when in a leadership position
        Epic, //Really good quality trait
        Rare, //Mid-tier trait
        Normal //Makes a unit more unique
    }
}
