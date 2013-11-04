using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tactics {
   public class Menu {
        public static SpriteFont Font;
        public int Height;
        public string[] Choices; // Possible choices
        public int Selection; // Current selection index
        public bool Chosen; // Final choice
        public int ElapsedTime;

        public void Initialize(string[] choices) {
            Choices = choices;
            Height = choices.Count() * Font.LineSpacing;
            Console.WriteLine("Menu Height {0}", Height);
            Selection = 0;
        }

        public void Update(InputState input) {
            if (input.KeyPressed(Keys.Enter)) {
                Chosen = true;
            } else if (input.KeyPressed(Keys.Down)) {
                Selection += 1;
            } else if (input.KeyPressed(Keys.Up)) {
                Selection -= 1;
            }

            if (Selection < 0) Selection = Choices.Count()-1;
            if (Selection >= Choices.Count()) Selection = 0;
        }

        public void Draw(Vector2 pos, SpriteBatch spriteBatch) {
            for (var i = 0; i < Choices.Count(); i++) {
                if (i == Selection) {
                    spriteBatch.DrawString(Font, Choices[i], new Vector2(pos.X, pos.Y + i * Font.LineSpacing), Color.Green);
                } else {
                    spriteBatch.DrawString(Font, Choices[i], new Vector2(pos.X, pos.Y + i * Font.LineSpacing), Color.White);
                }
            }
        }
    }
}
