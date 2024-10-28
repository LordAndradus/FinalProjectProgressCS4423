using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/* TODO
 * Figure out a system to where I keep stats, but everything else transfers. IE the unit is a knight and I demote them back to militia, it'll keep CP progression in the knight class.
 * Tier system is where with each iteration, the unit is 3 times as strong 3^tierlevel. Making the investment to upgrade appealing
*/

[Serializable]
public class Unit
{
    [SerializeField] public Sprite spriteView;
    [SerializeField] public string Name;
    [SerializeField] public string UIFriendlyClassName;
    [SerializeField] public string Description = "Make sure to fill in the description, young game maker";
    [SerializeField] public Sprite Icon;
/*     public static Dictionary<Type, List<Type>> PromotionMap = new(){
        { typeof(MilitiaMale), new(){ typeof(Footman), typeof(Spearman), typeof(JourneymanMage), typeof(Yeoman), typeof(Knight), typeof(Chasseur), typeof(Archer) }},
        { typeof(MilitiaBowman), PromotionMap[typeof(MilitiaMale)] },
        { typeof(MilitiaFemale), new(){ typeof(Sorceress), typeof(Priestess), typeof(Commando) }},
        { typeof(NeophyteMagus), new(){ typeof(JourneymanMage), typeof(Footman), typeof(Spearman), typeof(Yeoman) }},
    }; */

    [SerializeField] public MoveType movement;

    [Header("Base Attribute Scores")]
    //Base Threat = 100. Calculation goes as follows: Threat += 3 per normal stat, += 1 per armor & weapon; Threat += 1000 * tier level; Threat += 100 * Per Traits rarity
    [SerializeField] int Threat = 100;
    [SerializeField] int HP = 100, MaxHP = 100;
    [SerializeField] int Armor = 100;
    [SerializeField] int WeaponPower = 100;
    [SerializeField] int Strength = 100;
    [SerializeField] int Agility = 100;
    [SerializeField] int Magic = 100;
    [SerializeField] int Leadership = 100;
    [SerializeField] int FieldCost = 10; //base cose = 10; Mercenary = 12 to loyal = 8

    [Header("Equipment and Trait Additions")]
    [SerializeField] List<int> HPAdd = new();
    [SerializeField] List<int> ArmorAdd = new();
    [SerializeField] List<int> WeaponPowerAdd = new();
    [SerializeField] List<int> StrengthAdd = new();
    [SerializeField] List<int> AgilityAdd = new();
    [SerializeField] List<int> MagicAdd = new();
    [SerializeField] List<int> LeadershipAdd = new();

    [Header("Total Attributes")]
    [SerializeField] int TotalHP = 100;
    [SerializeField] int TotalArmor = 100;
    [SerializeField] int TotalWeapon = 100;
    [SerializeField] int TotalStrength = 100;
    [SerializeField] int TotalMagic = 100;
    [SerializeField] int TotalAgility = 100;
    [SerializeField] int TotalLeadership = 100;

    //This is only for promotion classes that require certain attributes
    [Header("Required Attribute Scores")]
    [SerializeField] int StrengthRequirement;
    [SerializeField] int AgilityRequirement;
    [SerializeField] int MagicRequirement;

    [Header("Attribute Growth Factors")]
    //Range of growth is from 1.0f to 5.0f - This affects the BASE stats of a unit
    //Savant tier is if the total growth is around 22f, Genius is 15f, Above-Average is 12f, Average is 10f, Poor is 8f, and Farmer is 5f 
    [SerializeField] GrowthType Growth; //Averaged based on growth scores. Tiers: Savant, Genius, Above-Average, Average, Below-Average, Poor, Farmer
    [SerializeField] float HPGrowth;
    [SerializeField] float DexterityGrowth;
    [SerializeField] float MagicGrowth;
    [SerializeField] float LeadershipGrowth;

    [Header("Attribute Generation Range")]
    [SerializeField] Pair<float, float> HPRange;
    [SerializeField] Pair<float, float> DexterityRange;
    [SerializeField] Pair<float, float> MagicRange;
    [SerializeField] Pair<float, float> LeadershipRange;

    [Header("Progession Meters")]
    [SerializeField] int Level;
    [SerializeField] int TierLevel;
    [SerializeField] int PromotionPoints;
    [SerializeField] int ExperiencePoints;

    [Header("Progression Caps Per Level")]
    [SerializeField] int PromotionCap; //Tier 1 = 500, Tier 2 = 3000, Tier 3 = 4500, MaxTier = 8000
    [SerializeField] int ExperienceCap; //The higher the tier, the higher the experience cap, yet the growth rate is multiplied by the tier

    //Cost when adding to squad, and when trying to spawn it
    [Header("Material Cost")]
    [SerializeField] public int GoldCost; 
    [SerializeField] public int IronCost;
    [SerializeField] public int MagicGemCost;
    [SerializeField] public int HorseCost;

    [Header("WIP Material Cost")]
    [SerializeField] public int HolyTearCost;
    [SerializeField] public int AdamntiumCost;

    [Header("Traits")]
    public List<Trait> traits;
    public static readonly int AbsoluteMaxTraits = 6;
    [SerializeField] public int MaxTraits = 6; //How many traits the unit can learn in its lifetime. Absolute maxmium is 6

    public Unit() {}

    public string displayQuickInfo()
    {
        return string.Format("{0}\nSTR: {1}\nAGI: {2}\nMAG: {3}\nLDR: {4}\nFCC: {5}\nGRO: {6}", Name, Strength, Agility, Magic, Leadership, FieldCost, Growth.ToString());
    }
    
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append(string.Format("My nane is {0}; class = {1}; I have {2} traits\n", Name, GetType().Name, traits.Count));

        foreach(Trait trait in traits) sb.Append("Trait = " + trait.ToString() + "\n");

        return sb.ToString();
    }

    public class Snapshot
    {
        Type Career;
        public int PromotionPoints;
        public int PromotionCap;
    }

    public void LevelUp()
    {
        HP += (int) HPGrowth;
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        TotalHP = HP + HPAdd.Sum();
        TotalArmor = Armor + ArmorAdd.Sum();
        TotalWeapon = WeaponPower + WeaponPowerAdd.Sum();
        TotalStrength = Strength + StrengthAdd.Sum();
        TotalMagic = Magic + MagicAdd.Sum();
        TotalAgility = Agility + AgilityAdd.Sum();
        TotalLeadership = Leadership + LeadershipAdd.Sum();
    }

    //Getters and Setters for Attribute Scores
    public int GetThreat() { return Threat; }
    public int GetHealth() { return HP; }
    public int GetMaxHealth() { return MaxHP; }
    public int GetArmor() { return Armor; }
    public int GetWeapon() { return WeaponPower; }
    public int GetStrength() { return Strength; }
    public int GetAgility() { return Agility; }
    public int GetMagic() { return Magic; }
    public int GetLeadership() { return Leadership; }

    public int GetLevel() { return Level; }
    public int GetFieldCost() { return FieldCost; }

    public int GetXPCap() { return ExperienceCap; }
    public int GetPPCap() { return PromotionCap; }
    public int GetXP() { return ExperiencePoints; }
    public int GetPP() { return PromotionPoints; }
}

public enum GrowthType
{
    Savant,
    Genius,
    Gifted,
    Avgerage,
    Subpar,
    Poor,
    Talentless
}

public enum MoveType
{
    Standard,
    Light,
    Slow,
    Cavalry,
    LightCavalry,
    Flying
}