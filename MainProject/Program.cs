using System;
using MainProject.Logic;

namespace MainProject
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new Station()) game.Run();
        }
    }
}
