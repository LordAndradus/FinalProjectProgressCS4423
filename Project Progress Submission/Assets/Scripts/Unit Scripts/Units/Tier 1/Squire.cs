using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Squire : Unit
{
    public Squire()
    {
        UIFriendlyClassName = "Squire";
        spriteView = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Squire/Squire");
        Icon = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Squire/Squire Icon");
        movement = MoveType.Cavalry;

        GoldCost = 175;
        HorseCost = 1;
    }
}
