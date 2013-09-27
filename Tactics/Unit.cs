using System;
using Microsoft.Xna.Framework.Graphics;

namespace Tactics {
    public class Unit {
        public Texture2D Texture;
        public int X;
        public int Y;

        public Unit() {
        }

        public void Initialize(Texture2D texture) {
            Texture = texture;

        }
    }
}

