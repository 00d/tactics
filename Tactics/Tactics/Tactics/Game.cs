#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using TiledSharp;
using System.Media;


#endregion

namespace Tactics
{
    public class Settings {
        public static Color OVERLAY_BLUE = new Color(89, 180, 255, 200);
        public static Color OVERLAY_RED = new Color(248, 104, 104, 200);
    }

	/// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        public enum GameState { SELECT_UNIT, ORDER_UNIT_MOVE, ORDER_UNIT_ACTION }

        public GraphicsDeviceManager GraphicsManager;
        public GraphicsDevice Graphics;
        public SpriteBatch SpriteBatch;	
        public Map Map;
        public Rectangle Viewport;
        public List<Unit> Units = new List<Unit>();
        public Texture2D Overlay;
        public GameState State = GameState.SELECT_UNIT;
        public Team PlayerTeam = Team.BLUE;
        public SpriteFont font;
        public Menu ActiveMenu;

        public Unit SelectedUnit;

        public Tile MouseTile; // Whichever tile the player is currently moused over

        public Game()
        {
            Content.RootDirectory = "Content";

            GraphicsManager = new GraphicsDeviceManager(this);
            GraphicsManager.PreferredBackBufferHeight = 768;
            GraphicsManager.PreferredBackBufferWidth = 1024;
            GraphicsManager.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            Viewport = Graphics.Viewport.Bounds;
            Viewport.X = 0;
            Viewport.Y = 0;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Graphics = GraphicsManager.GraphicsDevice;
            SpriteBatch = new SpriteBatch(Graphics);
            Map = new Map();
            Map.Initialize(this, new TmxMap("Content/Map/tiletest.tmx"));
            Overlay = Content.Load<Texture2D>("clear");

            var unit = new Unit();
            unit.Initialize(Content.Load<Texture2D>("Sprite/testunit"));
            unit.Map = Map;
            unit.MoveDistance = 10;
            unit.Team = Team.BLUE;
            unit.Put(Map.Tiles[30, 40]);
            Units.Add(unit);

            var enemy = new Unit();
            enemy.Initialize(Content.Load<Texture2D>("Sprite/testenemy"));
            enemy.Map = Map;
            enemy.MoveDistance = 10;
            enemy.Team = Team.RED;
            enemy.Put(Map.Tiles[10, 5]);
            Units.Add(enemy);

            var backMusic = Content.Load<Song>("Music/HonorableCombat");
            MediaPlayer.Volume = 1.0f;
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(backMusic);

            Menu.Font = Content.Load<SpriteFont>("SpriteFont1");
        }
        /*
        private void LoadGriddy() {
            Stream gridDataStream = new FileStream("Content/Maps/demoLevel.tmx", FileMode.Open, FileAccess.Read);
            Stream tileBankStream = new FileStream("Content/Maps/tileBank.xml", FileMode.Open, FileAccess.Read);

            GridData gridData = GridData.NewFromStreamAndWorldPosition(gridDataStream, new Vector2(1,0));
            TileBank tileBank = TileBank.CreateFromSerializedData(tileBankStream, Content);

            gridDataStream.Position = 0;
            SerializedGridFactory gridFactory = SerializedGridFactory.NewFromData(gridDataStream, gridData, tileBank);

            level = Grid.NewGrid(gridData, gridFactory, DefaultGridDrawer.NewFromGridData(gridData, Content, Color.Black));

            //set the window size to the size of the level. 
            graphics.PreferredBackBufferWidth = gridData.BoundingBox.Width;
            graphics.PreferredBackBufferHeight = gridData.BoundingBox.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }*/

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputState input = InputState.GetState(gameTime);

            if (input.KeyPressed(Keys.Escape))
                Exit();

            if (input.KeyPressed(Keys.Down))
                Viewport.Y += 1;
            if (input.KeyPressed(Keys.Up))
                Viewport.Y -= 1;
            if (input.KeyPressed(Keys.Right))
                Viewport.X += 1;
            if (input.KeyPressed(Keys.Left))
                Viewport.X -= 1;

            var bounds = Graphics.Viewport.Bounds;   

            // Calculate screen dimensions in tiles
            var tScreenWidth = bounds.Width / Map.Tmx.TileWidth;
            var tScreenHeight = bounds.Height / Map.Tmx.TileHeight;

            // Bound viewport to map edges
            Viewport.X = Math.Min(Viewport.X, Map.tMapWidth - tScreenWidth);
            Viewport.Y = Math.Min(Viewport.Y, Map.tMapHeight - tScreenHeight);
            Viewport.Width = tScreenWidth;
            Viewport.Height = tScreenHeight;

            // Bound to positive coordinates
            Viewport.X = Math.Max(0, Viewport.X);
            Viewport.Y = Math.Max(0, Viewport.Y);

            var tMouseX = Viewport.X + input.Mouse.X / Map.Tmx.TileWidth;
            var tMouseY = Viewport.Y + input.Mouse.Y / Map.Tmx.TileHeight;

            MouseTile = Map.Tiles[0, 0];
            if (Map.WithinBounds(tMouseX, tMouseY)) {                
                MouseTile = Map.Tiles[tMouseX, tMouseY];               
            }


             if (State == GameState.SELECT_UNIT) {
                if (input.KeyPressed(Keys.Enter)) {
                    EndTurn();
                } else {
                    foreach (var unit in MouseTile.Units) {
                        if (!unit.Moved && unit.Team == PlayerTeam && input.Mouse.LeftButton == ButtonState.Pressed) {
                            SelectedUnit = unit;
                            State = GameState.ORDER_UNIT_MOVE;
                        }
                    }
                }
            }
            else if (State == GameState.ORDER_UNIT_MOVE) {
                if (input.Mouse.LeftButton == ButtonState.Pressed && MouseTile != SelectedUnit.Tile) {
                    var path = Map.PathBetween(SelectedUnit, MouseTile);
                    if (path != null && path.Count() - 1 <= SelectedUnit.MoveDistance) {
                        SelectedUnit.Put(MouseTile);

                        // Open the action menu
                        var menu = new Menu();
                        menu.Initialize(new string[] { "Ability", "Wait" });
                        ActiveMenu = menu;
                        State = GameState.ORDER_UNIT_ACTION;
                    }
                }
             } else if (State == GameState.ORDER_UNIT_ACTION) {
                 ActiveMenu.Update(input);

                 if (ActiveMenu.Chosen) {
                     if (ActiveMenu.Selection == 0) {
                     } else {
                         SelectedUnit.Moved = true;
                         State = GameState.SELECT_UNIT;
                     }
                 }
             }

            // TODO: Add your update logic here			
            base.Update(gameTime);
        }

        public void EndTurn() {
            foreach (var unit in Units) {
                unit.Moved = false;
            }
        }

        /// <summary>
        /// Find the screen coordinates for a given gameplay tile.
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="tile">Tile.</param>
        public Vector2 ScreenPos(Tile tile) {
            return new Vector2(tile.X * Map.Tmx.TileWidth, tile.Y * Map.Tmx.TileHeight);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	Graphics.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();
            Map.Draw(SpriteBatch, Viewport);
            if (State == GameState.SELECT_UNIT && MouseTile != null) {
                SpriteBatch.Draw(Overlay, ScreenPos(MouseTile), Settings.OVERLAY_BLUE);              
                Console.WriteLine("{0} {1} {2} {3}", MouseTile.X, MouseTile.Y, MouseTile.Tmx.Gid, MouseTile.Flags.Obstacle);
            } else if (State == GameState.ORDER_UNIT_MOVE) {
                Tile destTile = null;

                foreach (var tile in SelectedUnit.PathMap()) {
                    SpriteBatch.Draw(Overlay, ScreenPos(tile), Settings.OVERLAY_BLUE);
                    if (tile == MouseTile && SelectedUnit.CanPass(tile)) {
                        destTile = tile;
                    }
                }

                if (destTile != null) {                    
                    foreach (var pathtile in Map.PathBetween(SelectedUnit, destTile)) {
                        SpriteBatch.Draw(Overlay, ScreenPos(pathtile), Color.Green);
                    }
                    SpriteBatch.Draw(SelectedUnit.Texture, ScreenPos(destTile), new Color(255, 255, 255, 175));
                }
            }
          
            foreach (var unit in Units) {
                if (unit.Moved) {
                    SpriteBatch.Draw(unit.Texture, ScreenPos(unit.Tile), Color.Gray);
                } else {
                    SpriteBatch.Draw(unit.Texture, ScreenPos(unit.Tile), Color.White);
                }
            }

            if (State == GameState.ORDER_UNIT_ACTION) {
                var spos = ScreenPos(SelectedUnit.Tile);
                ActiveMenu.Draw(new Vector2((float)(spos.X + Map.Tmx.TileWidth*1.5), spos.Y - ActiveMenu.Height/2), SpriteBatch);
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }

    }
}

