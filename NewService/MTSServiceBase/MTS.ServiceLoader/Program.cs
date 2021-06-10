using System.ServiceProcess;

namespace ServiceLoader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceLoader()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
