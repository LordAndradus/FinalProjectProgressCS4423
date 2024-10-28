using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sworn : Trait
{
    public Sworn()
    {
        FlavorText = "Despite still earning their coin, this soldier is starting to resolve themselves to fight for your cause!";
        EffectText = "-1 Field Cost and a 25% reduction in Gold cost to field";
    }
    
    public override void add()
    {
        
    }
}
