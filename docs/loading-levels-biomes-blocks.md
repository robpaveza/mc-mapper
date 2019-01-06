This file describes my reverse engineering of the level.

## Local Filesystem
Maps are stored in the UWP app location for a given user:

    %userprofile%\appdata\local\packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds

The subdirectories are named apparently with a short base64 string.  I have not attempted to reverse-engineer the meanings of these strings, if there is any.

## Entry point: level.dat
Level.dat is an NBT file.  The most interesting named tags in this file, so far as I am concerned, are `SpawnX`, `SpawnZ`, `LevelName`, and `RandomSeed`.  It occurs to me that `LevelName` may not be present as a tag because there is usually a file that lives next to the level.dat file called `levelname.txt`, but I could also imagine this being an optimization.

I also like to look at `Difficulty`, `lastLoadedWithVersion`, and `GameType`.

### Types: Difficulty, GameType

```csharp
enum Difficulty {
    Peaceful = 0,
    Easy = 1,
    Normal = 2,
    Hard = 3,
}

enum GameMode {
    Survival = 0,
    Creative = 1,
    Adventure = 2,
    /* unsupported in Bedrock so far */ Spectator = 3,
}
```

There appear to be a number of optimizations, or ways of being clever, that Minecraft can take advantage of which is a big drawback for our goals:

 - The level does not appear to contain X or Z boundaries; rather, whenever a player approaches a chunk coming into render distance, the client can query for the existence of that chunk by its coordinates, and then either load it from the database or generate it on demand.
 - There is not a single list of players. Rather, when a player connects to a server, they are either interpreted as `~local_player` or by a GUID associated with their XBL account. Because LevelDB lookups are fast, who  cares?

Adjacent to `level.dat`, there is a directory called `db` which contains the LevelDB database.  The instance of LevelDB used by Bedrock Edition is a fork of Google's which supports ZLib (rather than Snappy) compression, and can run on Windows.  See [their Github repo](https://github.com/mojang/leveldb-mcpe) for more information.

## Concepts of a Minecraft level
 - The fundamental unit of geometry in Minecraft is a _block_.  Blocks are 1x1.  Blocks get a 4-bit (a _nibble_) of data associated with them called _block data_; for example, Stone, identified as `minecraft:stone` in modern worlds, has block data`=0`; but Granite is `minecraft:stone` with block data`=1`.
   - It's unclear why we'd overload Stone perhaps (especially since Cobblestone isn't part of that), but this becomes super-useful for colored blocks like Stained Glass, Wool, and Beds.  A nibble is adequate to represent 16 options.
 - World areas are stored in the database as a _chunk_.  A chunk is a 16x16 area determined by its X and Z coordinates: increasing X is East, and increasing Z is South.  Chunks are the fundamental unit of "simulation" in Minecraft; if a chunk is loaded by any player, the whole chunk receives ticks from the game engine.
 - Geometry is split into _subchunks_, which are 16-blocks vertically tall.  Therefore, a subchunk contains 16x16x16 blocks, or 4,096 blocks.
 - Anything mobile is an _entity_.  Entities are things like mobs, players, armor stands, primed TNT, etc.
 - Then there are _block entities_, which are blocks that have additional data attached to them.  These can be as mundane as signs, or as complex as Hoppers or Dispensers.

With that as a primer, let's get to some nitty gritty details.

## Locating chunk data
LevelDB functions as a fast key-value lookup system; both keys and values are blob data (arbitrary byte streams).

Suppose that a player connects and hasn't played a level before, and the spawn is at (0, 0).  The server will spawn the player around the spawn coordinates, which will prompt the spawning area to be loaded.  A number of things have to be loaded assuming that chunk is unloaded at that time:

 - The heightmap needs to be loaded, to determine an appropriate `Y` coordinate to spawn the player
 - The server needs to send the client the geometry of the level
 - The server needs to load any mobs in the area, and begin ticking their AI
 - The server needs to process any redstone circuitry in the area
 - The server needs to load the biome data and determine if they need to affect mob spawning or weather effects

etc. etc.  For the moment we're interested in:
 - Biome layout
 - Block layout (or level geometry)
 - Maybe the heightmap to get the "overview" view

OK.  Take a look at the [Bedrock Edition level format](https://minecraft.gamepedia.com/Bedrock_Edition_level_format) documentation.  This is unclear, honestly, but here's how we construct a key:

```csharp
enum DataQueryTag : byte
{
    Data2D = 45, // heightmap, biome, (historical) block lighting
    Subchunk = 47, // More on this below
    BlockEntity = 49, // hoppers, droppers,  etc.
    Entity = 50, // Mobs, dropped items, etc.
    // see others listed on the documentation linked above
}
```

The `Data2D` tag is quite easy: on modern levels, it is 768 bytes long.  The first 512 bytes are an array of 256 `uint16_t` values, little-endian, containing the heightmap (this is because the build limit is actually y=256).  The following 256 bytes are biome IDs, increasing first by X and then by Z.  For chunk `0, 0`, the expression `biomeList[1]` would represent the biome at block `1, 0`.  That's great, let's examine how to get there.

The level format documentation talks about how to construct that key but I found it to be confusing until I started iterating over the keys.  Here's how it's constructed:

```
int32_t         chunk_x; // little-endian
int32_t         chunk_z; // little-endian
uint8_t         tag = 45
```

In other words, if you want to look up the biome and heightmap for the chunk located at block coordinates `4160,-16`, you'd translate that to chunk `260,-1`, which would be formatted to the following byte array:

    04 01 00 00 ff ff ff ff 2d
    ^X          ^Z          ^Tag

The first four bytes are `0x0104` (decimal 260) and the second four bytes are `0xffffffff` (or [two's complement -1](https://en.wikipedia.org/wiki/Two%27s_complement)), followed by the tag.

If the LevelDB query for this value returns null, it means that the chunk does not exist.

However, we're not done.  What about the Nether or The End?  They have the same coordinate system as the Overworld, so storing the keys the same way would mean they would overlay!  Well that's true, so before the tag type, chunks in the Nether or in the End have an extra uint32.  That's constructed as:

```
int32_t         chunk_x; // little-endian
int32_t         chunk_z; // little-endian
uint32_t        dimension; // little-endian
uint8_t         tag = 45
```

```csharp
enum Dimension {
    TheNether = 1,
    TheEnd = 2,
}
```

Thus, querying for the same block coordinate as above (`4160,-16`) in The End would be rendered out as:

    04 01 00 00 ff ff ff ff 02 00 00 00 2d
    ^X          ^Z          ^Dim        ^Tag


## Subchunk (Terrain) data 
You should definitely read the preceding section before this, or you'll be missing some crucial context.

Querying for a subchunk is a little more involved.  After specifying that the data block you're querying for is a subchunk, you also have to append the Y-index of the subchunk you're looking for, a value from 0 to 15 inclusive (or 0-7 in the Nether).  The y-index is multiplied by 16 to get the y-level for a particular block; or, the y-level for a particular block is divided by 16 to get its subchunk index.

### Querying for a subchunk
To query for a subchunk, you construct a key in the same manner as above, but append the subchunk index to the end of the key.  Remembering that the Subchunk tag is `47`, or in hex `0x2f`, querying for the subchunk containing the Overworld block at `4160,62,-16` (`x,y,z` coordinates) can be calculated by:

 - `y=62` is part of the 4th subchunk (0-15, 16-31, 32-47, **48-63**), so its index, being 0-based, is `3`.
 - Since we're in the Overworld, we don't care about the long-form

So it gets produced into the following:

    04 01 00 00 ff ff ff ff 2f 03
    ^X          ^Z       Tag^  ^Subchunk index

### Interpreting a subchunk data
Tommaso [documented the subchunk format changes](https://gist.github.com/Tomcc/a96af509e275b1af483b25c543cfbf37) in a Gist on Github.  There is some complexity here because Mojang wanted to preserve compatability with tools like MCEdit, which overwrote the version tag of the subchunk data, they had to do some funky business.  The first byte of the subchunk is always the version:

#### 1.2 format (pre-Update Aquatic)
Values of `0, 2, 3, 4, 5, 6,` or `7` indicate the pre-Update-Aquatic (1.2) file format.  The remainder of the data in this format are block IDs and block data in the form of byte arrays and nibble arrays.  Written like a C structure, it might look like:

```c
const uint16_t BLOCKS_PER_SUBCHUNK = 4096;

typedef uint8_t block_id;
// note: no padding
typedef struct tagSubchunk {
    uint8_t         version; // = 0, 2, 3, 4, 5, 6, or 7
    block_id        blocks[BLOCKS_PER_SUBCHUNK];
    uint8_t         blockData[BLOCKS_PER_SUBCHUNK / 2];
} Subchunk;
```

You need to know about this format if you're reading from a level that was created before Update Aquatic!  Chunk geometries aren't updated unless they're written to - and even then, I'm not sure if they're updated to the new version unless something that can't be expressed by the old version (that is, new blocks) gets written to the chunk.

#### Update Aquatic Beta (palettized version=1)
During UA Beta, Mojang introduced the palettized format.  This allows for unlimited* numbers of types of blocks to be stored in a given chunk (the astute observer will notice that subchunks above are limited to 256 block types and 16 subtypes with a maximum of 4096 different possible blocks ever).  

In this format, Mojang introduced two new types: the palette compression type, and the BlockStorage type.  The palette compression type enum indicates the number of bit per block for a given 32-bit unsigned integer:

```csharp
enum PaletteType : byte {
    Paletted1bpb = 1, // 32 blocks per uint32, 1 bit per block; 512 bytes storage
    Paletted2bpb = 2, // 16 blocks per uint32, 2 bits per block; 1024 bytes storage
    Paletted3bpb = 3, // 10 blocks per uint32, 3 bits per block; plus 2 bits unused padding; 1640 bytes storage
    Paletted4bpb = 4, // 8 blocks per uint32, 4 bits per block; 2048 bytes storage
    Paletted5bpb = 5, // 6 blocks per uint32, 5 bits per block; plus 2 bits unused padding; 2732 bytes storage
    Paletted6bpb = 6, // 5 blocks per uint32, 6 bits per block; plus 2 bits unused padding; 3280 bytes storage
    Paletted8bpb = 8, // 4 blocks per uint32, 8 bits per block; 4096 bytes storage
    Paletted16bpb = 16, // 2 blocks per uint32, 16 bits per block; 8192 bytes storage
}
```

This design allows for the 4096-block-storage to have as few as 512 bytes or as many as 8192.  It also means that the number of blocks in the region dictates the size of the block storage area.

Before I talk about how the palettized layout works, I'm going to first talk about the BlockStorage structure so that the entirety makes sense in context.

```c
typedef struct tagBlockStorage {
    // note: no padding
    bool                        usesRuntimeIds : 1; // least significant bit
    PaletteType                 compressionType : 7; 
    uint32_t                    blockIndices[calculated_size_from_palette_type];
    uint32_t                    paletteSize; // note: Tommaso's doc says this is a varint.  Maybe over network?
    NBTTag_t                    palette[paletteSize]; // for persistence, since that's what we care about for this project
} BlockStorage;
```

This can't be really represented by a C struct (it would be with a union), but you should be able to get the idea:
 
 - `usesRuntimeIds` and `compressionType` share a byte.  All of the values of compression are bit-shifted left by 1 in order to make room for the flag indicating whether we're using runtime IDs.
 - The size of `blockIndices` is variable based on the compression type chosen
 - In contrast to Tommaso's documentation, the `paletteSize` field appears to be a `uint32`, not a `varint`.
 - The `palette` itself is a stream of NBT tags, not an NBT file.  This is an important distinction!  An NBT file like level.dat contains a single root tag; this, instead, is a series of NBT-formatted tags.  The format of these tags is consistent: each entry is a `Compound` tag which contains two values: a `String` tag named `name` which contains the block type in the new format (such as `minecraft:stone`); and a `Short` value containing the block data.
   - Notably, the consequence of THIS design is that if you use, for example, each color of wool in a given subchunk, you'll end with 16 entries of `minecraft:wool` in the palette, one with each color.

The block indices are then indexes into the palette, which might change from subchunk to subchunk.  

Finally, the version 1 subchunk view is finally presented as:

```c
typedef struct tagVersion1Subchunk {
    uint8_t             version = 1;
    BlockStorage        blockStorage;
} Version1Subchunk;
```

#### Palette Compression
The compression is challenging to read.  Here are the characteristics:

 - The basic unit of index is the uint32.
 - The values go from least-significant byte to most-significant byte.
    - For example, given a Paletted16bpb-compressed datum, the first item is masked by `0xffff`, and the second item is masked by `0xffff0000`.
 - The padding bits are in the most-significant bits.

This is implemented in the `SubchunkCompression` class but it's honestly not very straightforward.  We rarely need to do this level of bit manipulation, particularly challenging for the 3-, 5-, and 6-bit-width representations.  I may write up a design doc of how I managed this in a future document.

#### Update Aquatic final release (palettized version=8)
For the final release, Mojang changed the block storage to be able to be an array of block storages:

```c
typedef struct tagVersion8Subchunk {
    uint8_t             version = 8;
    uint8_t             numStorages;
    BlockStorage        blockStorageList[numStorages];
}
```

Tomasso notes that multiple block storages are simply used to store water presently, which suggests to me that this is about waterlogged blocks.  At the moment, I ignore block storages past the first; in the future, this will probably yield a second image layer or something to that effect.

### After Decompression

After decompressing the block palette, you have a 4096-item array, either directly of block IDs (if the old version) or of palette entry indices.  To normalize access, since palletization is the "new thing," I precalculate and store the block palette based on the blocks present in non-palletized chunks and surface a single view of this (from the `SubchunkView` class).

The items in the indices increase one-by-one first by Y, then by Z, and finally by X, which is precisely the opposite of what I was hoping for.  Assuming we're looking at subchunk (0, 0, 0), then:

 - `blocks[1]` corresponds to `(0,1,0)`.
 - `blocks[17]` corresponds to `(0, 1, 1)`
 - `blocks[256]` corresponds to `(1, 0, 0)`
 - `blocks[257]` corresponds to `(1, 1, 0)`
 - `blocks[273]` corresponds to `(1, 1, 1)`

This is unfortunate for our purposes, since we want to present either a layer-by-layer (so a Y-plane) or a random-access .

## Final thoughts
I'll add additional documents about rendering, entities, block entities, etc. as these things get worked out.
