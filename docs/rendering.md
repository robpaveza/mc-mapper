This document outlines how rendering works.

## General data model

Bitmap graphics are our best destination; one pixel mapping to one block is going to be a good fit because:

 - In Minecraft, the smallest unit of building is a block
 - Biomes, heightmaps, and lighting are all computed per-block

The general model of a bitmap fits very well to Minecraft.  A bitmap is a  2-dimensional plane comprised of `x,y` coordinates in which the origin `0,0` is the top-left, positive-X is moving right, and positive-Y is moving down.  In a similar correlation, Minecraft is positive-X moving east and positive-Z moving south.  So, we render our map components such that `(X,Z)=>(x,y)`.

