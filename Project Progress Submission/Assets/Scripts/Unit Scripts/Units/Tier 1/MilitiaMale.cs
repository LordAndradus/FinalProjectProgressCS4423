using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MilitiaMale : Unit
{
    public MilitiaMale()
    {
        UIFriendlyClassName = "Fighter";
        spriteView = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Fighter/Fighter2");
        Icon = UtilityClass.Load<Sprite>("Sprites/Unit Sprites/Tier 1/Fighter/Fighter Icon");
        movement = MoveType.Standard;

        GoldCost = 100;
    }
}
