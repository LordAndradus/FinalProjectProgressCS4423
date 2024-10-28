using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class UnitResourceManager
{
    public static int Gold = 0;
    public static int Iron = 0;
    public static int MagicGems = 0;
    public static int Horses = 0;

    [Serializable]
    public class Wrapper
    {
        public int Gold, Iron, MagicGems, Horses;

        public Wrapper()
        {
            Gold = UnitResourceManager.Gold;
            Iron = UnitResourceManager.Iron;
            MagicGems = UnitResourceManager.MagicGems;
            Horses = UnitResourceManager.Horses;
        }
    }
}