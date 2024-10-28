using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalLeader : Trait
{
    public NaturalLeader()
    {
        rank = TraitRank.Rare;
        FlavorText = "Someone who is always at the center of their clique, making the tough decision no one else wants to.";
        EffectText = "When employed as a squad leader, they decreases the field cost of each unit by 10%";
    }

    public override void add()
    {
        
    }
}
