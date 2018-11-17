/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.ServiceProcess;
using Nancy.Hosting.Self;

namespace BatMon
{
    public partial class BatMonService : ServiceBase
    {
        NancyHost host;

        public BatMonService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            host = new NancyHost(Framework.Web.InterfaceWeb.GetUriBindings(BatMonPluginManager.Settings.Port));
            host.Start();
        }

        protected override void OnStop()
        {
            host.Stop();
        }
    }
}
