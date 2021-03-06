This document outlines the relationship of mc-mapper to mcpe_viz.

## Technical relationship, backend

There isn't much relationship in the backend components of mcpe_viz and mc-mapper.  mcpe_viz is written in C++; mc-mapper is written in C# with the exception of the LevelDB implementation (which is currently an external component).  From a cursory examination, mcpe_viz also does not appear to construct a data model over the Minecraft map.  Rather, it seems too just go directly from map to data.

## Technical relationship, frontend

From a technical perspective, the frontend isn't particularly related.  mcpe_viz's frontend is generated as a template using Gen1 JavaScript libraries (jQuery); mc-mapper is using Gen3 (React).

## Technical relationship, tiles and rendering

We're, as near as possible, using the same colors as exist in mcpe_viz to achieve the first iteration of biome and map generation.  The reason for this is to allow us to compare the outputs of mc-mapper generated maps to those generated by mcpe_viz.

One observation about mcpe_viz is that it uses the specific, exact color of the output pixel, as rendered on the map, to present the block-info.  This is a clever approach and one that we may adopt as the way of doing so.  I am concerned that it creates tight coupling between the rendering configuration parameters and the frontend; but, because the parameters are configurable, I think that's an acceptable trade.  I'm also concerned about the number of blocks vs. the number of possible colors.  This will be something I continue to investigate.

In the medium-long term, we want to create a set of themes which can be applied, possibly even automatically generated.  For example: suppose we have the Natural Texture Pack.  This pack looks generally similar to Vanilla, but notably, Jungle Wood's bark is gray, things appear generally more saturated, etc.  We should be able to run a tool against the Natural Texture Pack and create a render configuration such that the average of a block's pixels corresponds to the output color of that block on the map.

Having done so, we will greatly diverge from mcpe_viz's inspired rendering configuration.
