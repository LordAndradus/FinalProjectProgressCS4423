using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heroic : Trait
{
    public Heroic()
    {
        rank = TraitRank.Epic;
        FlavorText = "A soldier that will forge their path forward no matter how hopeless the situation. They exude a unique aura that inspires others to move forwards.";
        EffectText = "Defies a death blow once per battle. When defied, it increases the attack power of other squadmates by 100% when they next strike.";
    }
    
    public override void add()
    {
        
    }
}
