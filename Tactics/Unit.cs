using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Tactics {
    public enum Team { BLUE, RED, GREEN }

    public class Unit {
        public Map Map;
        public Texture2D Texture;
        public Tile Tile;
        public int X {
            get { return Tile.X; }
        }
        public int Y {
            get { return Tile.Y; }
        }

        public int MoveDistance; // Number of tiles unit can move per turn
        public Team Team;
        public bool Moved = false;
       
        public Unit() {
        }
              
        public void Initialize(Texture2D texture) {
            Texture = texture;
        }

        public bool CanPass(Tile tile) {
            foreach (var unit in tile.Units) {
                if (!IsAlly(unit)) {
                    return false;
                }
            }

            return !tile.Flags.Obstacle;
        }

        public bool IsAlly(Unit unit) {
            return unit.Team == Team;
        }


        /// <summary>
        /// Calculates pathmap for user interface.
        /// </summary>
        /// <returns>List of all tiles reachable in MoveDistance steps.</returns>
        public List<Tile> PathMap() {
            var seenTiles = new List<Tile>();

            var heads = new List<Tile>();
            var length = 0;
            heads.Add(Tile);

            while (length < MoveDistance) {
                var newHeads = new List<Tile>();

                foreach (var tile in heads) {
                    foreach (var neighbor in Map.NeighborsAround(tile)) {
                        if (CanPass(neighbor) && !seenTiles.Contains(neighbor)) {
                            seenTiles.Add(neighbor);
                            newHeads.Add(neighbor);
                        }
                    }
                }

                heads = newHeads;
                length += 1;
            }

            return seenTiles;
        }

        public void Put(Tile tile) {
            if (Tile != null) {
                Tile.Units.Remove(this);
            }

            Tile = tile;

            Tile.Units.Add(this);
        }
    }
}

