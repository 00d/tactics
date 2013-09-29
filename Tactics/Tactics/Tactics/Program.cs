#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Tactics
{

    static class Program
    {
        public static Game Game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Game = new Game();
            Game.Run();
        }
    }
}
