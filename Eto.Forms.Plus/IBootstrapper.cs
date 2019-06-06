namespace Eto.Forms.Plus
{
    public interface IBootstrapper
    {
        /// <summary>
        /// Called during application setup, allowing the bootstrapper to configure itself.
        /// Should hook into Application.Startup
        /// </summary>
        /// <param name="application">Reference to the current application</param>
        void Setup(Application application);
    }
}