using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tactics {
    // A system for running time-sensitive, repeating code
    public class Animation {
        public double ElapsedTime = 0; // Time in milliseconds since last frame
        public double FrameInterval = 100; // Number of milliseconds between each frame
        public int Frame = 0; // Which frame we're up to
        public bool BlockInput = true; // Whether this blocks user input
        public Action<int> FrameAction; // Callback for each frame
        public Action<SpriteBatch> DrawAction; // Optional callback for drawing
        public Action EndAction; // Callback for end of animation

        public void Update(GameTime gameTime) {
            ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (ElapsedTime > FrameInterval) {
                NextFrame();
                ElapsedTime = 0;
            }
        }

        public void NextFrame() {
            Frame += 1;
            FrameAction(Frame);
        }

        public void OnFrame(Action<int> frameAction) {
            FrameAction = frameAction;
        }

        public void OnEnd(Action endAction) {
            EndAction = endAction;
        }

        public void OnDraw(Action<SpriteBatch> drawAction) {
            DrawAction = drawAction;
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (DrawAction != null) {
                DrawAction(spriteBatch);
            }
        }

        public void Start() {
            Program.Game.Animations.Add(this);
        }

        public void Stop() {
            Program.Game.Animations.Remove(this);
            EndAction();
        }
    }
}
