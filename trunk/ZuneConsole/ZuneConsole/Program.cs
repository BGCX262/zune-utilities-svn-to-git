using System;

namespace ZuneConsole
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ConsoleTest game = new ConsoleTest())
            {
                game.Run();
            }
        }
    }
}
