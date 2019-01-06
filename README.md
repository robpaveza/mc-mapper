# mc-mapper
A mapper web server for Minecraft (Bedrock Edition) levels

## Disclosure
I, Rob Paveza, am an employee of Microsoft Corporation.  However, I am not a member of the Minecraft team;
the contents of this repository do not represent any official statement about the format of Minecraft 
(what we all call Bedrock Edition) levels, or any of the sort.  You should refer to the wiki, to tweets 
of the various team members, etc. for official statements to those effects.  Also, please bear in mind
that while this works today, it's not guaranteed (I tend to think of level format as being an "internal
implementation detail" for games).

## Current Status
At present, we can generate map tiles for biomes.

![CC09 Minecraft - (0,0) to (1023,1023)](./images/biomes_Overworld_0.0.png "CC09 Minecraft biomes for (0,0) to (1023,1023)")

## What is this?
I first got Minecraft during something like 1.2.  After I got blown up by my first creeper, like everyone
else, I went and checked out a FAQ, and loved it.  I quit though, after a short while, and that was that.

Until somehow, YouTube made the suggestion that I look at something called Hermitcraft.  It was a Mumbo 
Jumbo Hermitcraft Season 5 video, and my mind was blown.  Watching people play on a server was completely
different.  They had their shenanigans, etc. etc., and it wasn't long before I nagged a friend into a
Realm with me.  When he retired, I got hooked up with the Realm hosted by a Microsoft Retail Store in 
Los Angeles, and I was hooked.

One cool thing I'd discovered with my friend was [mcpe_viz](https://github.com/Plethora777/mcpe_viz).  This
was a pretty cool project that let me do stuff like find biomes that I might have missed while exploring,
find those tricky end cities, figure out of my server buddy was duping stuff, etc.  mcpe_viz has a TON of 
detail!

But.... it hasn't been updated since Update Aquatic.  It's also painfully slow depending on configuration
parameters.  The defaults, which create layer-by-layer images of all three dimensions, are absolutely 
tremendous, but with a cost: My 40mb realm would generate about 2.5gb of map tiles at a run-time of about 
3 hours.  But that run-time was worth the cost when the data were up-to-date.  Since then, the level 
formats have changed (chunk block data are now palettized), a ton of new blocks were added (all corals),
many blocks can be waterlogged, there are new trapdoors, etc. etc. and on and on.

This project attempts to address those issues, at a cost:  You have to run it as a webserver.

## Project components
_MCBEWorld_ is the bottom tier of the project.  It is the part that interacts with LevelDB, has data 
structures that closely correlate to the level format, and, wherever possible, presents a level of
abstraction that doesn't impose a performance penalty.

MCBEWorld will also contain a library capable of reading NBT data.  One day, it may also include native
binaries for LevelDB; for now, that isn't a goal.

_MCBERenderer_ is the, shockingly, rendering component of the project.  Its purpose is to separate out
the concern of graphics from the level details.  It should be able to be driven from a command line,
web server, or other kind of application equally well.  Its interface knows about the building blocks of
Minecraft worlds: Chunks and subchunks, blocks, and biomes; and it accepts data to allow customization
of rendering (such as if you wanted to "theme" your rendering based on a particular texture pack, for
instance).

_MCBEMapper_ is the "web API" component of the project.  Effectively, it will translate web requests
to particular paths into either PNG images or GeoJSON responses.  In many ways, this is the primary
glue between MCBEWorld and MCBERenderer.

_MCBEViewer_ is a React web application that will mirror mcpe_viz's look and feel.  Taking a nod from
mcpe_viz, we will use OpenLayers as the primary driver of the map; using an orthographic projection,
we can accurately represent the view of a Minecraft world by simply stitching together the blocks as
pixels or multi-pixel views.

## Goals
 - v1: Be able to generate an interactive, tiled map using OpenLayers, on demand, representing either the biomes, the world visible from space, or for a particular y-level
 - Be able to display information about signs
 - Be able to display information about entities that might be relevant (mobs, players)
 - Rendering the nether: Ignore the topmost solid when rendering the topmost view
 - Apply a shaded relief on top of normal terrain
 - Overlays for chunk grid, slime chunks, nether portals, end portals

## How does it work?
I will be writing up design documents as we go and documenting my findings.  See the [docs folder](./docs) for more information about that.

## Interesting Tools
 - [mcpe_viz](https://github.com/Plethora777/mcpe_viz) is still pretty cool if you have a copy of a world you haven't updated recently
 - [MCC Toolchest PE](http://www.mcctoolchest.com/Download) was very useful when I was validating how I understood the world to be stored.  However, be wary when using this tool; it presents world data as if it is in regions and lists of players, which are not accurate for Bedrock (probably to match MCC Toolchest for Java Edition's UI behavior).