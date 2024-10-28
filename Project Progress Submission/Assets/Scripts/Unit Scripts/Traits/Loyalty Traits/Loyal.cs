using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Loyal : Trait
{
    public Loyal()
    {
        FlavorText = "This soldier has gone through hell and back, they truly understand the weight of this cause!";
        EffectText = "-2 Field Cost and a 50% reduction in Gold cost to field";
    }

    

    public override void add()
    {
        Debug.Log("The most loyal of soldiers!");
    }

    public override void remove()
    {
        Debug.Log("Removing this trait!");
    }
}
