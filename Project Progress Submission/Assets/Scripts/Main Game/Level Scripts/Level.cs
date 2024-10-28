using System;
using System.Collections.Generic;
using System.Numerics;

[Serializable]
public class Level
{
    public PathFinder pf;
    public CombatGridSystem cgs;

    //Bounds -> First = Negative boundary, Second = Positive boundary : The bounds are in terms of how many tiles
    public static Pair<float, float> MapSize { get; set; }

    public Level()
    {

    }

    public Level(Pair<float, float> MapSize)
    {

    }

    public void LoadLevelSquadlist()
    {
        //Load squad list from current Save File
    }
}