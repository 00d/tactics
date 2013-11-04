using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tactics {
    public enum TargetStyle { AOE_PROJECTILE };

    // Represents a particular action that can be performed during battle
    public class Ability {
        public Game Game;
        public TargetStyle TargetStyle = TargetStyle.AOE_PROJECTILE;

        public Ability() {
            Game = Program.Game;
        }

        public Animation Invoke(Unit user, Tile target) {
            var path = Game.Map.PathBetween(user, target);
            var sprite = new Sprite("fireball");

            var anim = new Animation();

            anim.BlockInput = true;
            anim.FrameInterval = 50;

            anim.OnFrame((frame) => {
                if (path.Count == 0) {
                    anim.Stop();
                    return;
                }

                var tile = path[0];
                path.RemoveAt(0);

                var pos = Game.ScreenPos(tile);
                sprite.X = pos.X;
                sprite.Y = pos.Y;
            });

            anim.OnDraw((spriteBatch) => {
                spriteBatch.Draw(sprite.Texture, new Vector2(sprite.X, sprite.Y), Color.White);      
            });

            anim.Start();

            return anim;
        }
    }
}
