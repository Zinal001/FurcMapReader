# FurcMapReader
A C# .NET 4.0 project for reading Furcadia Map Files


This library consists of 3 classes:
* Map
* MapPosition
* MapTile

# Map
 The Map class is the hearth of the library, it can read map files, make new ones and change objects/floors/walls etc on any tile on the map.

# MapPosition
 The MapPosition class represents a position on the map, it contains all available position data:
 * Floor Number
 * Object Number
 * Wall Number
 * Region Number
 * Effect Number
 
# MapTile
 The MapTile class is very similar to the MapPosition class but with one differance. The MapTile contains 2 wall properties, both for NE and NW.
##### Why is this neccessary?
This can be a helpful class for people who are going to create another map editor for Furcadia. Each tile on the Dream Editor are really 2 positions:
<br />One position contains the floor/object/North-Eastern wall/region and effect while the other position contains ONLY the North-Western wall.
