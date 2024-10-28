using System;
using UnityEngine;

public class Tutorial : Level
{    
    public Tutorial()
    {
        MapSize = new(26, 26);
        cgs = new CombatGridSystem((int) MapSize.First, (int) MapSize.Second);
    }
}