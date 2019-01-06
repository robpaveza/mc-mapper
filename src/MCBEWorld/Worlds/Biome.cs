using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public class Biome
    {
        public Biome(string friendlyName, string dataName, int id)
            : this(friendlyName, dataName, id, true)
        {

        }

        public Biome(int id)
            : this($"Unknown Biome {id}", $"unknown_biome_{id}", id, false)
        {

        }

        private Biome(string friendlyName, string dataName, int id, bool isKnown)
        {
            FriendlyName = friendlyName;
            DataName = dataName;
            ID = id;
            IsKnown = isKnown;
        }

        public bool IsKnown { get; }
        public string FriendlyName { get; }
        public string DataName { get; }
        public int ID { get; }
    }

    public static class Biomes
    {
        internal static Dictionary<int, Biome> KnownBiomes = new Dictionary<int, Biome>();

        public static readonly Biome Ocean = new Biome("Ocean", "ocean", 0);
        public static readonly Biome DeepOcean = new Biome("Deep Ocean", "deep_ocean", 24);
        public static readonly Biome FrozenOcean = new Biome("Frozen Ocean", "frozen_ocean", 10);
        public static readonly Biome DeepFrozenOcean = new Biome("Deep Frozen Ocean", "deep_frozen_ocean", 50);
        public static readonly Biome ColdOcean = new Biome("Cold Ocean", "cold_ocean", 46);
        public static readonly Biome DeepColdOcean = new Biome("Deep Cold Ocean", "deep_cold_ocean", 49);
        public static readonly Biome LukewarmOcean = new Biome("Lukewarm Ocean", "lukewarm_ocean", 45);
        public static readonly Biome DeepLukewarmOcean = new Biome("Deep Lukewarm Ocean", "deep_lukewarm_ocean", 48);
        public static readonly Biome WarmOcean = new Biome("Warm Ocean", "warm_ocean", 44);
        public static readonly Biome DeepWarmOcean = new Biome("Deep Warm Ocean", "deep_warm_ocean", 47);
        public static readonly Biome River = new Biome("River", "river", 7);
        public static readonly Biome FrozenRiver = new Biome("Frozen River", "frozen_river", 11);
        public static readonly Biome Beach = new Biome("Beach", "beach", 16);
        public static readonly Biome StoneShore = new Biome("Stone Shore", "stone_shore", 25);
        public static readonly Biome SnowyBeach = new Biome("Snowy Beach", "snowy_beach", 26);
        public static readonly Biome Forest = new Biome("Forest", "forest", 4);
        public static readonly Biome WoodedHills = new Biome("Wooded Hills", "wooded_hills", 18);
        public static readonly Biome FlowerForest = new Biome("Flower Forest", "flower_forest", 132);
        public static readonly Biome BirchForest = new Biome("Birch Forest", "birch_forest", 27);
        public static readonly Biome BirchForestHills = new Biome("Birch Forest Hills", "birch_forest_hills", 28);
        public static readonly Biome TallBirchForest = new Biome("Tall Birch Forest", "tall_birch_forest", 155);
        public static readonly Biome TallBirchHills = new Biome("Tall Birch Hills", "tall_birch_hills", 156);
        public static readonly Biome DarkForest = new Biome("Dark Forest", "dark_forest", 29);
        public static readonly Biome DarkForestHills = new Biome("Dark Forest Hills", "dark_forest_hills", 157);
        public static readonly Biome Jungle = new Biome("Jungle", "jungle", 21);
        public static readonly Biome JungleHills = new Biome("Jungle Hills", "jungle_hills", 22);
        public static readonly Biome ModifiedJungle = new Biome("Modified Jungle", "modified_jungle", 149);
        public static readonly Biome JungleEdge = new Biome("Jungle Edge", "jungle_edge", 23);
        public static readonly Biome ModifiedJungleEdge = new Biome("Modified Jungle Edge", "modified_jungle_edge", 151);
        public static readonly Biome BambooJungle = new Biome("Bamboo Jungle‌", "bamboo_jungle", 168);
        public static readonly Biome BambooJungleHills = new Biome("Bamboo Jungle Hills", "bamboo_jungle_hills‌", 169);
        public static readonly Biome Taiga = new Biome("Taiga", "taiga", 5);
        public static readonly Biome TaigaHills = new Biome("Taiga Hills", "taiga_hills", 19);
        public static readonly Biome TaigaMountains = new Biome("Taiga Mountains", "taiga_mountains", 133);
        public static readonly Biome SnowyTaiga = new Biome("Snowy Taiga", "snowy_taiga", 30);
        public static readonly Biome SnowyTaigaHills = new Biome("Snowy Taiga Hills", "snowy_taiga_hills", 31);
        public static readonly Biome SnowyTaigaMountains = new Biome("Snowy Taiga Mountains", "snowy_taiga_mountains", 158);
        public static readonly Biome GiantTreeTaiga = new Biome("Giant Tree Taiga", "giant_tree_taiga", 32);
        public static readonly Biome GiantTreeTaigaHills = new Biome("Giant Tree Taiga Hills", "giant_tree_taiga_hills", 33);
        public static readonly Biome GiantSpruceTreeTaiga = new Biome("Giant Spruce Taiga", "giant_spruce_taiga", 160);
        public static readonly Biome GiantSpruceTaigaHills = new Biome("Giant Spruce Taiga Hills", "giant_spruce_taiga_hills", 161);
        public static readonly Biome MushroomFields = new Biome("Mushroom Fields", "mushroom_fields", 14);
        public static readonly Biome MushroomFieldShore = new Biome("Mushroom Field Shore", "mushroom_field_shore", 15);
        public static readonly Biome Swamp = new Biome("Swamp", "swamp", 6);
        public static readonly Biome SwampHills = new Biome("Swamp Hills", "swamp_hills", 134);
        public static readonly Biome Savanna = new Biome("Savanna", "savanna", 35);
        public static readonly Biome SavannaPlateau = new Biome("Savanna Plateau", "savanna_plateau", 36);
        public static readonly Biome ShatteredSavanna = new Biome("Shattered Savanna", "shattered_savanna", 163);
        public static readonly Biome ShatteredSavannaPlateau = new Biome("Shattered Savanna Plateau", "shattered_savanna_plateau", 164);
        public static readonly Biome Plains = new Biome("Plains", "plains", 1);
        public static readonly Biome SunflowerPlains = new Biome("Sunflower Plains", "sunflower_plains", 129);
        public static readonly Biome Desert = new Biome("Desert", "desert", 2);
        public static readonly Biome DesertHills = new Biome("Desert Hills", "desert_hills", 17);
        public static readonly Biome DesertLakes = new Biome("Desert Lakes", "desert_lakes", 130);
        public static readonly Biome SnowyTundra = new Biome("Snowy Tundra", "snowy_tundra", 12);
        public static readonly Biome SnowyMountains = new Biome("Snowy Mountains", "snowy_mountains", 13);
        public static readonly Biome IceSpikes = new Biome("Ice Spikes", "ice_spikes", 140);
        public static readonly Biome Mountains = new Biome("Mountains", "mountains", 3);
        public static readonly Biome WoodedMountains = new Biome("Wooded Mountains", "wooded_mountains", 34);
        public static readonly Biome GravellyMountains = new Biome("Extreme Hills", "extreme_hills", 131);
        public static readonly Biome GravellyMountainsPlus = new Biome("Extreme Hills+", "extreme_hills_plus_trees", 162);
        public static readonly Biome MountainEdge = new Biome("Mountain Edge", "mountain_edge", 20);
        public static readonly Biome Badlands = new Biome("Badlands", "badlands", 37);
        public static readonly Biome BadlandsPlateau = new Biome("Badlands Plateau", "badlands_plateau", 39);
        public static readonly Biome ModifiedBadlandsPlateau = new Biome("Modified Badlands Plateau", "modified_badlands_plateau", 167);
        public static readonly Biome WoodedBadlandsPlateau = new Biome("Wooded Badlands Plateau", "wooded_badlands_plateau", 38);
        public static readonly Biome ModifiedWoodedBadlandsPlateau = new Biome("Modified Wooded Badlands Plateau", "modified_wooded_badlands_plateau", 166);
        public static readonly Biome ErodedBadlands = new Biome("Eroded Badlands", "eroded_badlands", 165);
        public static readonly Biome Nether = new Biome("Nether", "nether", 8);
        public static readonly Biome TheEnd = new Biome("The End", "the_end", 9);
        public static readonly Biome SmallEndIslands = new Biome("Small End Islands", "small_end_islands", 40);
        public static readonly Biome EndMidlands = new Biome("End Midlands", "end_midlands", 41);
        public static readonly Biome EndHighlands = new Biome("End Highlands", "end_highlands", 42);
        public static readonly Biome EndBarrens = new Biome("End Barrens", "end_barrens", 43);
        public static readonly Biome TheVoid = new Biome("The Void", "the_void", 127);
    }
}
