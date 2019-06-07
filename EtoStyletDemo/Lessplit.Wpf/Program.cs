using Eto.Forms;
using System;

namespace Demo.Wpf
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run(new Application(Eto.Platforms.Wpf));
        }
    }
}
