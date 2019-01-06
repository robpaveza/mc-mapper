This document outlines supported API paths for the MCBEMapper project (the web backend).

## Worlds

### Get world info
`GET /world`

Returns: JSON object


## Biomes

### Render chunk

`GET /biome/chunk?dim={int}&chunkX={int}&chunkZ={int}`

Parameters:

 - `dim`: Dimension identifier (0 = Overworld, 1 = The Nether, 2 = The End)
 - `chunkX` and `chunkZ`: Chunk coordinates

Returns: 16x16 PNG image representing the biomes for the given chunk

### Render region

`GET /biome/tile1024?dim={int}&tileX={int}&tileZ={int}`

Parameters:

 - `dim`: Dimension identifier
 - `tileX` and `tileZ`: Region coordinates (a region is 64 chunks wide by 64 chunks tall)

Returns: 1024x1024 PNG image representing the biomes for the given region