/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */


using Topshelf.Nancy;
using Topshelf;
using NLog;
using System.Text;

namespace BatMon
{
    
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            var host = HostFactory.New(x =>
            {
                x.UseNLog();

                x.Service<BatMonService>(s =>
                {
                    s.ConstructUsing(settings => new BatMonService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                    s.WithNancyEndpoint(x, c =>
                    {
                        c.AddHost(port: BatMonPluginManager.Settings.Port);
                        c.CreateUrlReservationsOnInstall();
                        c.OpenFirewallPortsOnInstall(firewallRuleName: "BatMon");
                    });
                });
                x.StartAutomatically();
                x.SetServiceName("BatMon");
                x.SetDisplayName("BatMon (System Monitor)");
                x.SetDescription("Web Services providing JSON feeds for monitoring.");
                x.RunAsLocalSystem();
            });

            host.Run();
        }
    }
}
