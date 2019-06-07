using Eto.Forms.Plus;
using StyletIoC;

namespace Demo
{
    public class Bootstrapper : BootstrapBase<MainFormViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {

            // Configure the builder


        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
