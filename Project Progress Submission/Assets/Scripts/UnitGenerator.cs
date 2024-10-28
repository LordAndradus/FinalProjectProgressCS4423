using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitGenerator : MonoBehaviour
{
    public static Unit generate<T>() where T : Unit, new()
    {
        UnitFactory uf = new();

        Unit u = uf.get<T>(false);

        AssignUnitQualities(u);

        return u;
    }
    public static Unit generate()
    {
        UnitFactory uf = new();

        Unit u = uf.get(false);
        
        AssignUnitQualities(u);

        return u;
    }

    public static Squad GenerateSquad()
    {
        Squad generic = new Squad();
        for(int j = UnityEngine.Random.Range(1, 10); j > 0; j--) 
        {
            Unit u = generate();
            
            Pair<int, int> pair;

            do{
                pair = new Pair<int, int>(UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(0, 3));

                bool AlreadyFielded = false;
                foreach(var ugp in generic.RetrieveUnitPairs())
                {
                    if(ugp.Second.equals(pair)) AlreadyFielded = true;
                }

                if(AlreadyFielded) continue;
                
                break;
            }while(true);

            if(generic.RetrieveUnits().Count == 0) generic.Name = u.Name + "'s Squad";
            generic.FieldUnit(u, pair);
        }
        
        return generic;
    }

    private static void AssignUnitQualities(Unit u)
    {
        TraitFactory tf = new();
        
        u.traits = new();
        
        int numTraits = (int) UnityEngine.Random.Range(0.0f, (float) Unit.AbsoluteMaxTraits + 0.5f);

        for(int i = (int) UnityEngine.Random.Range(0.0f, numTraits); i < numTraits; i++)
        {
            Trait t = new();

            t = tf.get();

            if(t == null) break;

            if(u.traits.Contains(t))
            {
                i--;
                continue;
            }

            u.traits.Add(t);
        }
    }

    public static Unit generateMerc()
    {
        TraitFactory tf = new();
        UnitFactory uf = new();

        Unit u = uf.get(true);

        AssignUnitQualities(u);

        //Generate fake snapshots

        return u;
    }
}
class TraitFactory
{
    private readonly List<Func<Trait>> classes = new List<Func<Trait>>();
    private static readonly System.Random random = new System.Random();
    
    public TraitFactory()
    {
        buildFactory(() => new Loyal(),
                     () => new Mercenary(),
                     () => new Intelligent(),
                     () => new Heroic(),
                     () => new Sworn(),
                     () => new Bulwark(),
                     () => new NaturalLeader());
    }

    void buildFactory(params Func<Trait>[] classes)
    {
        this.classes.AddRange(classes);
    }

    public Trait get()
    {
        if(UnityEngine.Random.Range(0f, 1f) >= 0.5f) return new Learnable();
        else return classes[random.Next(classes.Count)](); 
    }
}

class UnitFactory
{
    private readonly List<Func<Unit>> Novice = new List<Func<Unit>>();
    private readonly List<Func<Unit>> Mercenaries = new List<Func<Unit>>();
    private static readonly System.Random random = new System.Random();
    
    public UnitFactory()
    {
        buildFactory(Novice,
                     () => new MilitiaMale(),
                     () => new MilitiaFemale(),
                     () => new MilitiaBowman(),
                     () => new Apothecary(),
                     () => new NeophyteMagus(),
                     () => new Squire());

        Mercenaries.AddRange(Novice);
    }

    void buildFactory(List<Func<Unit>> list, params Func<Unit>[] classes)
    {
        list.AddRange(classes);
    }

    public Unit get<T>(bool merc) where T : Unit, new()
    {
        Unit u;
        if(merc) u = Mercenaries[random.Next(Mercenaries.Count)]();
        else u = new T();

        AssignName(u);

        return u;
    }

    public Unit get(bool merc)
    {
        List<Func<Unit>> list = merc ? Mercenaries : Novice;

        Unit u = list[random.Next(list.Count)]();
        
        AssignName(u);

        return u;
    } 

    void AssignName(Unit u)
    {
        switch(u)
        {
            case NeophyteMagus:
            case MilitiaMale:
            case Squire:
            case MilitiaBowman:
                u.Name = UnitNamePicker.MaleNames[random.Next(UnitNamePicker.MaleNames.Length)];
                break;

            case MilitiaFemale:
            case Apothecary:
                u.Name = UnitNamePicker.FemaleNames[random.Next(UnitNamePicker.FemaleNames.Length)];
                break;

            default:
                u.Name = "Name not set";
                break;
        }
    }

    public class UnitNamePicker
    {
        public static readonly string[] MaleNames = new string[]{
            "Alaric", "Baldric", "Cedric", "Dorian", "Edric", "Fendrel", "Gareth", "Hadrian", "Isen", "Jareth", "Kael",
            "Loric", "Magnus", "Nolan", "Odran", "Perrin", "Quintus", "Roderic", "Seren", "Thorne", "Ulric", "Valen", "Weylin",
            "Xander", "Yorick", "Zarek", "Arlen", "Beric", "Calder", "Dain", "Eldrin", "Faelan", "Garrick", "Halston", "Ivar", 
            "Jorin", "Kiernan", "Leoric", "Morwen", "Norin", "Orion", "Percival", "Quillon", "Rathgar", "Stefan", "Torin", "Uther", 
            "Varric", "Wulfric", "Xavian", "Yvain", "Zane", "Althalos", "Borin", "Corwin", "Derrin", "Eldar", "Falric", "Gorin", 
            "Harken", "Ithran", "Jaric", "Kelden", "Lucan", "Marik", "Neric", "Othar", "Pryce", "Quellan", "Rolan", "Soren", 
            "Tarran", "Ulfran", "Varian", "Wyatt", "Xenos", "Yorin", "Zaric", "Ander", "Bran", "Caspian", "Draven", "Eirik", 
            "Finian", "Galen", "Helm", "Isran", "Jago", "Kendrick", "Leif", "Merrick", "Niall", "Orik", "Patrin", "Rhett", "Silas", 
            "Talion", "Viktor", "Aric", "Bram", "Cador", "Daelin", "Eldred", "Fenrir", "Godric", "Hagen", "Isidore", "Joren", "Kallus", 
            "Lorcan", "Marek", "Nestor", "Osric", "Padrig", "Quintin", "Ragnor", "Stellan", "Tybalt", "Ulfar", "Vardon", "Wendell", 
            "Xerxes", "Yorvan", "Zephan", "Alwin", "Briar", "Corvus", "Drystan", "Edmund", "Faramir", "Gawain", "Hawke", "Iskander", 
            "Jorin", "Kester", "Lysander", "Mordred", "Nikolai", "Owain", "Perrick", "Quillan", "Roderick", "Severin", "Torrin", 
            "Ulwin", "Valtor", "Warin", "Xanthos", "Yvain", "Zerik", "Armand", "Brander", "Colwyn", "Dorian", "Emric", "Fendrel", 
            "Garren", "Harland", "Ithor", "Jarrod", "Kaelar", "Landon", "Merrin", "Niall", "Orwin", "Pryor", "Quinton", "Renly", 
            "Sorrel", "Theron", "Ulman", "Varek", "Wulfran", "Xathar", "Yaron", "Zevran", "Alastair", "Bennet", "Caelum", "Doran", 
            "Elric", "Falcon", "Godwyn", "Helios", "Ingram", "Jareth", "Korin", "Lorian", "Myron", "Nyron", "Oberyn", "Phineas", 
            "Rath", "Soren", "Tavian", "Ulrick", "Valen", "Wystan", "Xavian"
        };

        public static readonly string[] FemaleNames = new string[]{
            "Aeliana", "Branwen", "Catrin", "Delara", "Elira", "Fiona", "Gwyneira", "Helena", "Isolde", "Jora", "Kaela", "Liora", 
            "Mira", "Nerina", "Orla", "Perin", "Quinara", "Rhiannon", "Selene", "Talia", "Ursa", "Valeria", "Wren", "Xanthe", 
            "Ysabella", "Zaria", "Adara", "Brynna", "Cerys", "Dahlia", "Eira", "Freya", "Giselle", "Hannelore", "Ilara", "Jessa", 
            "Karina", "Lilith", "Maelis", "Nimue", "Olwyn", "Phaedra", "Quilla", "Rowena", "Sable", "Tressa", "Ulyssa", "Vesna", 
            "Willow", "Xyla", "Ylva", "Zephira", "Anwen", "Bryony", "Celeste", "Damaris", "Elara", "Ferelith", "Genevieve", "Hedra", 
            "Inara", "Jolene", "Kiera", "Leora", "Melora", "Nyssa", "Ophelia", "Penna", "Quorra", "Ravenna", "Seraphine", "Thalassa", 
            "Ursula", "Viera", "Winry", "Xenia", "Yara", "Zanna", "Ariana", "Bellara", "Coralyn", "Daria", "Evelyn", "Fiora", "Giselle", 
            "Hestia", "Isara", "Jolina", "Kalira", "Leira", "Meridia", "Nerissa", "Odessa", "Primrose", "Raisa", "Sylara", "Tirian", "Ula", 
            "Vanya", "Winona", "Xalena", "Ysolde", "Zelena", "Aisling", "Briala", "Caelin", "Danica", "Elspeth", "Faylin", "Glenna", 
            "Havena", "Illyria", "Jessamine", "Kallista", "Lyanna", "Merewen", "Naida", "Oriana", "Portia", "Quinlynn", "Rosalind", 
            "Sabine", "Talwyn", "Una", "Vespera", "Wynn", "Xylia", "Yliana", "Zelara", "Arwen", "Briallen", "Calista", "Deryn", "Eldria", 
            "Fiora", "Gwenyth", "Halena", "Irina", "Jolessa", "Kairi", "Loralei", "Minara", "Nymera", "Ondine", "Phaelis", "Reya", "Selara", 
            "Thessaly", "Urith", "Vivianne", "Wynter", "Xelene", "Ysella", "Zira", "Aveline", "Brysa", "Carmine", "Drusilla", "Ember", 
            "Fiora", "Gwendolyn", "Hilde", "Idalia", "Jorina", "Kynthia", "Lirien", "Morgana", "Nolwenn", "Orlena", "Perwen", "Queline", 
            "Rhoswen", "Selina", "Tanith", "Ulina", "Vala", "Wisteria", "Xanna", "Ydelle", "Zephyra", "Avalora", "Belwyn", "Celestra", 
            "Deira", "Elysia", "Fayra", "Gloriana", "Helaine", "Ilara", "Jasira", "Kara", "Liora", "Miriel", "Nivara", "Olara", "Prynn", 
            "Riona", "Syrene", "Talindra", "Undine", "Vaella", "Willa", "Xelena", "Yloria", "Zorina"
        };

        public static readonly string[] DragonNames = new string[]{
            "Aldruin", "Balrath", "Calathor", "Draknar", "Eldryn", "Fyrnax", "Glaurung", "Hraxxis", "Ixthar", "Jorvax", "Kalthor", 
            "Lithyr", "Malygon", "Nithral", "Ormagon", "Pyros", "Quarion", "Rhaziel", "Sableclaw", "Talonis", "Urzalon", "Valthor", 
            "Wrathion", "Xaroth", "Ydrasyl", "Zaroth", "Arkalon", "Banehollow", "Corthis", "Draxis", "Emberwyrm", "Felorath", "Gornak", 
            "Haldros", "Ignathar", "Javrex", "Krazel", "Lokthar", "Malakar", "Nyxar", "Onyxius", "Pyrrion", "Quarthan", "Rauthien", 
            "Silrath", "Thaldrun", "Umbraloth", "Vexaris", "Wyrmscourge", "Xarvax", "Yrdalon", "Zephiran", "Ashandor", "Blazewing", 
            "Cyrath", "Droxar", "Eldrax", "Furion", "Grimtalon", "Hargrimm", "Iskador", "Jakaras", "Kalthorax", "Lorvath", "Morvyr", 
            "Nyxalis", "Othrax", "Pyralith", "Quirax", "Rhazalor", "Skaldor", "Tharvok", "Uldross", "Valtheris", "Wyrmbane", "Xarthen",
            "Yllendros", "Zyphar", "Andrakar", "Brimscar", "Caeloth", "Duskclaw", "Emberon", "Falsaar", "Gryndor", "Havorax", "Ishakar",
            "Jornath", "Karrath", "Lokmar", "Marathor", "Naldryn", "Orynth", "Pyrrhion", "Quorzal", "Rhazgar", "Sorath", "Thyrax", 
            "Uldor", "Velthros", "Wyrmrend", "Xerath", "Ysolth", "Zalthor"
        };

        public static readonly string[] UniqueUnitNames = new string[]{
            "General Razmuth", "Battle Master Jetaime", "Hawkeye Dantalion", "Namath the Wanderer"
        };
    } 
}