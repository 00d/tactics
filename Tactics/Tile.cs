using System;
using TiledSharp;
using System.Collections.Generic;



namespace Tactics {
    public class TileFlags {
        public bool Obstacle = false;
    }

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
        public TileFlags Flags = new TileFlags();
        public List<Unit> Units = new List<Unit>();

        public Tile() {
        }

        public void Initialize(TmxLayerTile tmxTile) {
            Tmx = tmxTile;
            X = Tmx.X;
            Y = Tmx.Y;
        }

        public void InitProps(PropertyDict tmxProps) {            
            foreach (var pair in tmxProps) {
                if (pair.Key == "obstacle") {
                    Flags.Obstacle = true;
                }
            }
        }
    }
}

