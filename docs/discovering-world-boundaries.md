This document talks about discovering world boundaries.

## Technical Limitations

Level files don't contain any details about which chunks have been explored.  There isn't a min/max coordinate space in level.dat, but that wouldn't really make sense anyway.  The LevelDB databases appear to contain entries about each dimension, but these entries appear to be related to abandoned entities.

In some ways, this is really clever.  Players dictate what regions are loaded; if the game engine knows where players are located, it can readily determine which chunks to load and calculate their DB keys (see [Loading Levels, Biomes, and Blocks](./loading-levels-biomes-blocks.md) for details on that), so the game doesn't need to know what chunks exist until they're asked for by players.  It just makes it difficult for our purposes!

## Approach

LevelDB has a concept called an *iterator* that operates over a snapshot of the database.  (A *snapshot* is a point-in-time readonly view of the database).  The iterator moves sequentially through the records and gives the user an opportunity to read the key and the value.

Since we know how to generate the key, we can also understand how to read the key back.  This will give us a number of X and Z coordinates for every dimension that has been explored.  Unfortunately, it will give us the same coordinates multiple times (once for each type tag for each chunk), but that will give us a start.

### Kinds of keys in LevelDB

There aren't only the chunk-related keys, however; there are a number of human-readable keys which contain various data.  Observed from a friend's large (~2.5gb) Realm world:

 - `AutonomousEntities`
 - `BiomeData`
 - `Nether`
 - `Overworld`
 - `TheEnd`
 - `dimension0`
 - `mVillages`
 - `map_<number>` (there are almost 2200 of these in our world)
 - `player_<number>` or `player_<guid>`
 - `player_server_<guid>`
 - `portals`
 - `tickingarea_<guid>`
 - `~local_player`

The worst part about these is that several of them might otherwise look like valid chunk keys.  `BiomeData` and `Overworld`, for example, are both 9 bytes long and could be mistaken for a chunk key.  We need a way to distinguish between human-readable strings and binary-encoded data that is fast, specifically, doesn't convert every key into a string and make sure it's human-readable.

### Interpreting Keys Quickly

Because the block dimensions are limited to 32-bit Two's Complement integers, we know that the range of the world is -2,147,483,648 to 2,147,483,647.  Because chunks represent 16 blocks, the maximum theoretical chunk coordinates are -134,217,728 to 134,217,727.  However, we can be a bit more clever than that.

In Bedrock Edition, glitches as a result of 32-bit floating point error accumulation begins to take place at about coordinates around 1,000,000.  The [Far Lands](https://minecraft.gamepedia.com/Far_Lands#Bedrock_Edition) begin to initiate at block coordinates of +/- 12,550,824; this position would be found in chunk 784,426, or represented in hex, `0x000bf82a` for the positive, `0xfff407d6`.

By observing whether a key contains `0x00` or `0xff` in `key[3]` or `key[7]` (because the keys are little-endian), we can determine with a high degree of accuracy that a key is a "named key" or "chunk data".  That is to say, keys containing `0x00` or `0xff` in those bytes are extremely likely to be chunk data; and all other keys are likely to be named keys.

## Representing the renderable area

In order to identify when a chunk is renderable, we're going to store a bitmap called a *region*.  A Region will contain a representation of a 64x64 area of chunks, or 4096 chunks; stored as a compressed bitfield (where 1 byte represents 8 chunks), each Region requires a 512-byte byte array.  A single Z-row of chunks, then, takes 8 bytes, and to determine whether a given chunk is present, first multiply the Z-index into the region by 8, divide the X-index by 8, and take the remainder of the X-index division to determine which bit to check.

An astute observe might notice that this can be optimized with:

 - Instead of dealing with bytes, deal with 32-bit integers (which the processor will do internally anyway)
 - Instead of dividing, since we're using power-of-2 values, we can bit-shift and mask

Traversing regions in this way allows us to do a couple of things which will be beneficial for our purposes:

 - We can skip trying to generate a chunk rendering altogether, instead rendering a black square
 - We can stash the boundaries to limit the client from looking too far

