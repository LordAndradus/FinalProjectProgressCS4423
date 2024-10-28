using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Apothecary : Unit
{
    public Apothecary()
    {
        UIFriendlyClassName = UtilityClass.UIFriendlyClassName(this.GetType().Name);
        spriteView = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Apothecary/Female Apothecary");
        Icon = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Apothecary/Apothecary Icon");
        movement = MoveType.Slow;

        GoldCost = 150;
        MagicGemCost = 1;
    }
}
