// Distributed as part of Tesserae, Copyright 2012 Marshall Ward
// Licensed under the Apache License, Version 2.0
// http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace Tactics
{
    public class Map
    {
        public Dictionary<TmxTileset, Texture2D> spriteSheet;
        public Dictionary<int, Rectangle> tileRect;
        public Dictionary<int, TmxTileset> idSheet;
        public Dictionary<int, PropertyDict> TileProps;

        public int tMapWidth, tMapHeight;

        // Temporary
        public TmxMap Tmx;          // TMX data (try to remove this)

        public RenderTarget2D renderTarget;
        public SpriteBatch batch;
       
        public List<Tile[,]> Layers; // Individual tile layers used for rendering
        public Tile[,] Tiles; // Combined gameplay tilemap

        public Game Game;

        public Map() {
        }

        public void Initialize(Game game, TmxMap tmxmap) {
            // Temporary code
            Game = game;
            Tmx = tmxmap;
            tMapWidth = Tmx.Width;
            tMapHeight = Tmx.Height;

            // Load spritesheets
            spriteSheet = new Dictionary<TmxTileset, Texture2D>();
            tileRect = new Dictionary<int, Rectangle>();
            idSheet = new Dictionary<int, TmxTileset>();
            TileProps = new Dictionary<int, PropertyDict>();

            foreach (TmxTileset ts in Tmx.Tilesets) {
                var newSheet = GetSpriteSheet(ts.Image.Source);
                spriteSheet.Add(ts, newSheet);

                foreach (TmxTilesetTile tile in ts.Tiles) {
                    TileProps[tile.Id] = tile.Properties;
                }

                // Loop hoisting
                var wStart = ts.Margin;
                var wInc = ts.TileWidth + ts.Spacing;
                var wEnd = ts.Image.Width - (ts.Image.Width % (ts.TileWidth + ts.Spacing));

                var hStart = ts.Margin;
                var hInc = ts.TileHeight + ts.Spacing;
                var hEnd = ts.Image.Height - (ts.Image.Height % (ts.TileHeight + ts.Spacing));

                // Pre-compute tileset rectangles
                var id = ts.FirstGid;
                for (var h = hStart; h < hEnd; h += hInc) {
                    for (var w = wStart; w < wEnd; w += wInc) {
                        var rect = new Rectangle(w, h,
                                                 ts.TileWidth, ts.TileHeight);
                        idSheet.Add(id, ts);
                        tileRect.Add(id, rect);
                        id += 1;
                    }
                }

                // Ignore properties for now
            }

            // Compute map structure and gameplay tiles
            // Individual layers are used for rendering
            // The combined layer represents the topmost tiles, and is used for gameplay

            Layers = new List<Tile[,]>();
            Tiles = new Tile[tMapWidth, tMapHeight]; // Combined layer

            foreach (TmxLayer tmxLayer in Tmx.Layers) {
                var layer = new Tile[tMapWidth, tMapHeight];

                foreach (TmxLayerTile tmxTile in tmxLayer.Tiles) {
                    var tile = new Tile();
                    tile.Initialize(tmxTile);
                    if (TileProps.ContainsKey(tmxTile.Gid-1)) { // HACK (Mispy): Don't ask me why this is necessary, I think it's a tiled bug
                        tile.InitProps(TileProps[tmxTile.Gid-1]);
                    }
                    layer[tile.X, tile.Y] = tile;
                    Tiles[tile.X, tile.Y] = tile;
                }

                Layers.Add(layer);
            }
        }

        public bool WithinBounds(int x, int y) {
            return (x >= 0 && y >= 0 && x < tMapWidth && y < tMapHeight);
        }
                       
        public double DistanceBetween(int x1, int y1, int x2, int y2) { 
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public double DistanceBetween(Tile tile1, Tile tile2) {
            return DistanceBetween(tile1.X, tile1.Y, tile2.X, tile2.Y);
        }

        /// <summary>
        /// Calculates a tiles representing "circle" approximation.
        /// </summary>
        /// <returns>List of tiles representing the circle.</returns>
        /// <param name="tile">Center tile.</param>
        /// <param name="radius">Radius of the circle.</param>
        public List<Tile> CircleAround(Tile tile, int radius) {
            var tiles = new List<Tile>();

            for (var i = tile.X-radius; i <= tile.X+radius; i++) {
                for (var j = tile.Y-radius; j <= tile.Y+radius; j++) {
                    if (WithinBounds(i, j) && DistanceBetween(tile.X, tile.Y, i, j) <= radius) {
                        tiles.Add(Tiles[i, j]);
                    }
                }
            }

            return tiles;
        }

        public List<Tile> NeighborsAround(Tile tile) {
            var tiles = new List<Tile>();

            foreach (var point in NeighborsAround(tile.X, tile.Y)) {
                if (WithinBounds(point.X, point.Y)) {
                    tiles.Add(Tiles[point.X, point.Y]);
                }
            }

            return tiles;
        }

        public Point[] NeighborsAround(int x, int y) {
            return new Point[]
            {
                new Point(x-1, y),
                new Point(x, y-1),
                new Point(x+1, y),
                new Point(x, y+1)
            };
        }

        public List<Tile> PathBetween(Unit unit, Tile dest) {
            return PathBetween(unit.Tile, dest, tile => unit.CanPass(tile));
        }

        public List<Tile> PathBetween(Tile start, Tile end, Func<Tile, bool> passable) {
            // nodes that have already been analyzed and have a path from the start to them
            var closedSet = new List<Tile>();
            // nodes that have been identified as a neighbor of an analyzed node, but have 
            // yet to be fully analyzed
            var openSet = new List<Tile> { start };
            // a dictionary identifying the optimal origin Tile to each node. this is used 
            // to back-track from the end to find the optimal path
            var cameFrom = new Dictionary<Tile, Tile>();
            // a dictionary indicating how far each analyzed node is from the start
            var currentDistance = new Dictionary<Tile, int>();
            // a dictionary indicating how far it is expected to reach the end, if the path 
            // travels through the specified node. 
            var predictedDistance = new Dictionary<Tile, float>();

            // initialize the start node as having a distance of 0, and an estmated distance 
            // of y-distance + x-distance, which is the optimal path in a square grid that 
            // doesn't allow for diagonal movement
            currentDistance.Add(start, 0);
            predictedDistance.Add(
                start, 
                0 + +Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y)
                );

            // if there are any unanalyzed nodes, process them
            while (openSet.Count > 0)
            {
                // get the node with the lowest estimated cost to finish

                var current = (
                    from p in openSet orderby predictedDistance[p] ascending select p
                ).First();

                // if it is the finish, return the path
                if (current.X == end.X && current.Y == end.Y)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, end);
                }

                // move current node from open to closed
                openSet.Remove(current);
                closedSet.Add(current);

                // process each valid node around the current node
                foreach (var neighbor in NeighborsAround(current))
                {
                    if (!passable(neighbor)) {
                        continue;
                    }

                    var tempCurrentDistance = currentDistance[current] + 1;

                    // if we already know a faster way to this neighbor, use that route and 
                    // ignore this one
                    if (closedSet.Contains(neighbor) 
                        && tempCurrentDistance >= currentDistance[neighbor])
                    {
                        continue;
                    }

                    // if we don't know a route to this neighbor, or if this is faster, 
                    // store this route
                    if (!closedSet.Contains(neighbor) 
                        || tempCurrentDistance < currentDistance[neighbor])
                    {
                        if (cameFrom.Keys.Contains(neighbor))
                        {
                            cameFrom[neighbor] = current;
                        }
                        else
                        {
                            cameFrom.Add(neighbor, current);
                        }

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] = 
                            currentDistance[neighbor] 
                            + Math.Abs(neighbor.X - end.X) 
                                + Math.Abs(neighbor.Y - end.Y);

                        // if this is a new node, add it to processing
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // unable to figure out a path, abort.
            return null;
        }

        /// <summary>
        /// Process a list of valid paths generated by the Pathfind function and return 
        /// a coherent path to current.
        /// </summary>
        /// <param name="cameFrom">A list of nodes and the origin to that node.</param>
        /// <param name="current">The destination node being sought out.</param>
        /// <returns>The shortest path from the start to the destination node.</returns>
        private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<Tile> { current };
            }

            var path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public void Draw(SpriteBatch batch, Rectangle viewport)
        {
            // Loop hoisting (Determined from Canvas)
            var iStart = viewport.X;
            var iEnd = viewport.X + viewport.Width;

            var jStart = viewport.Y;
            var jEnd = viewport.Y + viewport.Height;

            //Console.WriteLine("iStart {0} iEnd {1} jStart {2} jEnd {3}", iStart, iEnd, jStart, jEnd);

            // Draw tiles inside canvas
            foreach (var layer in Layers)
            {
                for (var i = iStart; i < iEnd; i++)
                {
                    for (var j = jStart; j < jEnd; j++)
                    {
                        //Console.WriteLine("i {0} j {1}", i, j);
                        var id = layer[i,j].Tmx.Gid;

                        // Skip unmapped cells
                        if (id == 0) continue;

                        // Pre-calculate? (not with tileScale in there...)
                        var position = new Vector2(
                            Tmx.TileWidth * (i - iStart),
                            Tmx.TileHeight * (j - jStart));

                        var tileset = spriteSheet[idSheet[id]];

                        batch.Draw(tileset, position,
                                   tileRect[id], Color.White, 0.0f, new Vector2(0,0),
                                   1, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public Texture2D GetSpriteSheet(string filepath)
        {
            Texture2D newSheet;
            Stream imgStream;


            imgStream = File.OpenRead(filepath);

            newSheet = Texture2D.FromStream(Game.Graphics, imgStream);
            return newSheet;
        }
    }
}