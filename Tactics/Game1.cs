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


#endregion

namespace Tactics
{
    public class Settings {
        public static Color OVERLAY_BLUE = new Color(89, 180, 255, 200);
    }

	/// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        public enum GameState { SELECT_UNIT, ORDER_UNIT }

        public GraphicsDeviceManager GraphicsManager;
        public GraphicsDevice Graphics;
        SpriteBatch spriteBatch;	
        Map map;
        Rectangle viewport;
        List<Unit> Units = new List<Unit>();
        int tMouseX;
        int tMouseY;
        Texture2D overlay;
        public GameState State = GameState.SELECT_UNIT;
        Unit SelectedUnit;

        public Game()
        {
            Content.RootDirectory = "Content";

            GraphicsManager = new GraphicsDeviceManager(this);
            GraphicsManager.PreferredBackBufferHeight = 768;
            GraphicsManager.PreferredBackBufferWidth = 1024;
            GraphicsManager.IsFullScreen = false;
            Graphics = GraphicsManager.GraphicsDevice;
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

            viewport = Graphics.Viewport.Bounds;
            viewport.X = 0;
            viewport.Y = 0;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            map = new Map();
            map.Initialize(this, new TmxMap("Content/Maps/tiletest.tmx"));
            overlay = Content.Load<Texture2D>("clear");

            var unit = new Unit();
            unit.Initialize(Content.Load<Texture2D>("Sprites/testunit.png"));
            unit.X = 30;
            unit.Y = 40;
            Units.Add(unit);

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
            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (keys.IsKeyDown(Keys.Escape))
                Exit();

            if (keys.IsKeyDown(Keys.Down))
                viewport.Y += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 16);
            if (keys.IsKeyDown(Keys.Up))
                viewport.Y -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 16);
            if (keys.IsKeyDown(Keys.Right))
                viewport.X += Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 16);
            if (keys.IsKeyDown(Keys.Left))
                viewport.X -= Convert.ToInt32(gameTime.ElapsedGameTime.TotalMilliseconds / 16);

            var bounds = Graphics.Viewport.Bounds;   

            // Calculate screen dimensions in tiles
            var tScreenWidth = bounds.Width / map.Tmx.TileWidth;
            var tScreenHeight = bounds.Height / map.Tmx.TileHeight;

            // Bound viewport to map edges
            viewport.X = Math.Min(viewport.X, map.tMapWidth - tScreenWidth);
            viewport.Y = Math.Min(viewport.Y, map.tMapHeight - tScreenHeight);
            viewport.Width = tScreenWidth;
            viewport.Height = tScreenHeight;

            // Bound to positive coordinates
            viewport.X = Math.Max(0, viewport.X);
            viewport.Y = Math.Max(0, viewport.Y);

            tMouseX = viewport.X + mouse.X / map.Tmx.TileWidth;
            tMouseY = viewport.Y + mouse.Y / map.Tmx.TileHeight;

            foreach (var unit in Units) {
                if (unit.X == tMouseX && unit.Y == tMouseY && mouse.LeftButton == ButtonState.Pressed) {
                    SelectedUnit = unit;
                    State = GameState.ORDER_UNIT;
                }
            }

            // TODO: Add your update logic here			
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	Graphics.Clear(Color.CornflowerBlue);
		
            var mouseX = tMouseX * map.Tmx.TileWidth;
            var mouseY = tMouseY * map.Tmx.TileHeight;
            var mousePos = new Vector2(mouseX, mouseY);

            spriteBatch.Begin();
            map.Draw(spriteBatch, viewport);
            if (State == GameState.SELECT_UNIT) {
                spriteBatch.Draw(overlay, mousePos, Settings.OVERLAY_BLUE);              
            } else if (State == GameState.ORDER_UNIT) {

            }

            foreach (var unit in Units) {
                var position = new Vector2(unit.X * map.Tmx.TileWidth, unit.Y * map.Tmx.TileHeight);
                spriteBatch.Draw(unit.Texture, position, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}

