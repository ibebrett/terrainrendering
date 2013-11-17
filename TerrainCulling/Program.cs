using System;

namespace TerrrainCulling
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TerrainCulling game = new TerrainCulling())
            {
                game.Run();
            }
        }
    }
}

