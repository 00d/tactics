// Distributed as part of Tesserae, Copyright 2012 Marshall Ward
// Licensed under the Apache License, Version 2.0
// http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

            foreach (TmxTileset ts in Tmx.Tilesets) {
                var newSheet = GetSpriteSheet(ts.Image.Source);
                spriteSheet.Add(ts, newSheet);

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
                    tile.X = tmxTile.X;
                    tile.Y = tmxTile.Y;
                    tile.Tmx = tmxTile;
                    layer[tile.X, tile.Y] = tile;
                    Tiles[tile.X, tile.Y] = tile;
                }

                Layers.Add(layer);
            }
        }


        public void Draw(SpriteBatch batch, Rectangle viewport)
        {
            // Loop hoisting (Determined from Canvas)
            var iStart = viewport.X;
            var iEnd = viewport.X + viewport.Width;

            var jStart = viewport.Y;
            var jEnd = viewport.Y + viewport.Height;

            Console.WriteLine("iStart {0} iEnd {1} jStart {2} jEnd {3}", iStart, iEnd, jStart, jEnd);

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

            var asm = Assembly.GetEntryAssembly();
            var manifest = asm.GetManifestResourceNames();

            var fileResPath = filepath.Replace(
                Path.DirectorySeparatorChar.ToString(), ".");
            var fileRes = Array.Find(manifest, s => s.EndsWith(fileResPath));
            if (fileRes != null)
                imgStream = asm.GetManifestResourceStream(fileRes);
            else
                imgStream = File.OpenRead(filepath);

            newSheet = Texture2D.FromStream(Game.Graphics, imgStream);
            return newSheet;
        }
    }
}