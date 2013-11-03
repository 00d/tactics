using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Tactics {
    public class Sprite {
        public float X;
        public float Y;
        public Texture2D Texture;

        public Sprite(string path) {
            Texture = Program.Game.Content.Load<Texture2D>("Sprite/" + path);
        }
    }
}
