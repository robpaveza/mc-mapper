This document outlines some data inconsistencies that have been found.

## Biome IDs

Biome IDs (the numbers that come from the `data2d` segment of the chunk data)
do not align perfectly with the values from the Minecraft wiki which only contain
the values from the Java version.  Specifically, biome IDs 40-43, which are biomes
pertaining to The End (Small End Islands, End Midlands, End Highlands, End Barrens)
do not appear to exist at all in Bedrock Edition.  Warm Ocean, and all of the 
subsequent biome IDs through Deep Frozen Ocean, move sequentially downward.

This results in the following changes:

 - Warm Ocean: 44 -> 40
 - Lukewarm Ocean: 45 -> 41
 - Cold Ocean: 46 -> 42
 - Deep Warm Ocean: 47 -> 43
 - Deep Lukewarm Ocean: 48 -> 44
 - Deep Cold Ocean: 49 -> 45
 - Deep Frozen Ocean: 50 -> 46

I have observed in at least one 1.7/1.8 world a biome ID of 47, which appears to be 
related to oceans.

## Biome data names

Differentiating between the data names outlined on the wiki (for example, such as `flower_forest`) and those discovered in the Texture Pack references is challenging.  What we typically think of as Extreme Hills is identified as `mountains` on the Wiki, but is referenced to as `extreme_hills` in the texture pack format's `biomes_client.json` file.  Unsure biome data names from the texture pack include:

 - `*_mutated` don't exist (perhaps matching `*_hills` instead?)
 - `hell` is likely `nether`
 - `ice_plains`, `ice_plains_spikes`, and `ice_mountains` - unsure how to match these
 - `mushroom_island` appears to be `mushroom_fields` and `mushroom_island_shore` appears to be `mushroom_fields_shore`
 - the list goes on
