using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using BatMon.Framework;

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
