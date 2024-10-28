using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mercenary : Trait
{
    public Mercenary()
    {
        FlavorText = "This soldier is only contractually obligated to aid you in your cause. Perhaps you can sway them to your side?";
        EffectText = "+2 Field Cost and a 100% increase in Gold cost to field";
    }
    
    public override void add()
    {
        
    }
}
