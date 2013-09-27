using System;
using TiledSharp;

namespace Tactics {
    /// <summary>
    /// Represents a single gameplay tile on a map. Some useful data
    /// like coordinates and contents are duplicated here for efficiency
    /// and ease of access (rather than having to e.g. iterate over all
    /// units to find which one is on a tile)
    /// </summary>
    public class Tile {
        public int X;
        public int Y;
        public TmxLayerTile Tmx; // Map editor source tile

        public Tile() {
        }
    }
}

