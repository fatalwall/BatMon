/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using Microsoft.VisualBasic.Devices;


namespace BatMon.Framework.Web
{
    public class InterfaceWeb : NancyModule
    {

        public static Uri[] GetUriBindings(int Port)
        {
            List<Uri> uriResults = new List<Uri>();
            uriResults.Add(new Uri(string.Format("http://{0}:{1}", "localhost", Port)));
            uriResults.Add(new Uri(string.Format("http://{0}:{1}", Dns.GetHostName(), Port)));
            if (Dns.GetHostEntry("").HostName != Dns.GetHostName())
                uriResults.Add(new Uri(string.Format("http://{0}:{1}", Dns.GetHostEntry("").HostName, Port)));
            foreach (string ip in NetworkInterface.GetAllNetworkInterfaces()
                                    .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || i.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                                    .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                                    .Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork && i.SuffixOrigin != SuffixOrigin.LinkLayerAddress)
                                    .Select(i => i.Address.ToString()))
            {
                uriResults.Add(new Uri(string.Format("http://{0}:{1}", ip, Port)));
            }
            return uriResults.ToArray();
        }

        private BatMonPluginManager pluginManager = BatMonPluginManager.getCurrentInstance;

        private string getFooter()
        {
            if(System.IO.File.Exists(@".\web\Views\footer.html"))
                return System.IO.File.ReadAllText(@".\web\Views\footer.html");
            else return "";
        }

        private string getHeader()
        {
            if (System.IO.File.Exists(@".\web\Views\header.html"))
                return System.IO.File.ReadAllText(@".\web\Views\header.html");
            else return "";
        }



        public InterfaceWeb()
        {
            Get["/"] = x =>
            {
                string t = "<link rel='stylesheet' href='/css/home.css' />";
                t = t + @"<script type='text/javascript' src='https://www.gstatic.com/charts/loader.js'></script>";
                t = t + @"<div class='Body'>";

                //bound uri
                t = t + @"<div class='WidgetWrapper'>";
                t = t + @"<div class='Widget' id='BoundUri'>";
                t = t + string.Format(@"<div class='header'>{0}</div>", "Addresses");
                foreach (var uri in GetUriBindings(BatMonPluginManager.Settings.Port))
                {
                    t = t + string.Format(@"<div class='link'><a href='{0}'>{0}</a></div>", uri);
                }
                t = t + @"</div>";
                t = t + @"</div>";


                //Health Report
                t = t + @"<div class='WidgetWrapperFill'>";
                t = t + @"<div class='Widget' id='HealthReport'>";
                t = t + string.Format(@"<div class='header'>{0}</div>", "Health Report");
                if (pluginManager.RunAll().Count > 0)
                {
                    t = t + @"<script type='text/javascript'>";
                    t = t + @"google.charts.load('current', { 'packages':['corechart']});";
                    foreach (var p in pluginManager.Plugins.Where(i => i.Value.Results.Values.Count > 0))
                    {
                        t = t + p.Value.htmlPieChart();
                    }
                
                    t = t + @"</script>";
                    t = t + "<div class='PieChartWrapper'>";
                    foreach (var p in pluginManager.Plugins.Where(i => i.Value.Results.Values.Count > 0))
                    {
                        t = t + string.Format("<div class='PieChart' id='{0}'></div>", p.Metadata.Name);
                    }
                    t = t + @"</div>";
                }
                t = t + @"</div>";
                t = t + @"</div>";

                //Server Info
                t = t + @"<div class='WidgetWrapper'>";
                t = t + @"<div class='Widget' id='ServerInfo'>";
                t = t + string.Format(@"<div class='header'>{0}</div>", "Server Info");
                t = t + string.Format(@"<div class='content'><b>{0}:</b> {1}</div>", "Host Name", System.Environment.MachineName);
                t = t + string.Format(@"<div class='content'><b>{0}:</b> {1}</div>", "OS", new ComputerInfo().OSFullName);
                t = t + string.Format(@"<div class='content'><b>{0}:</b> {1}</div>", "OS Platform", new ComputerInfo().OSPlatform);
                t = t + string.Format(@"<div class='content'><b>{0}:</b> {1}</div>", "OS Version", new ComputerInfo().OSVersion);
                t = t + string.Format(@"<div class='content'><b>{0}:</b> {1}</div>", "BatMon Version", this.GetType().Assembly.GetName().Version.ToString());

                t = t + @"<div class='content'>";
                t = t + @"<table class='System'>";
                t = t + @"<thead>";
                t = t + @"<tr>";
                t = t + @"<th>Cores</th>";
                t = t + @"<th>Memory</th>";
                t = t + @"<th>Usage</th>";
                t = t + @"</tr>";
                t = t + @"</thead>";
                t = t + @"<tbody>";
                var TotalMem = new ComputerInfo().TotalPhysicalMemory;
                var AvailMem = new ComputerInfo().AvailablePhysicalMemory;
                int MemPercent = (int)(((double)(TotalMem - AvailMem) / TotalMem) * 100);
                t = t + string.Format(@"<tr class='{0}'>", MemPercent >= 95 ? "Critical" : MemPercent >= 85 ? "Warning" : "Good");
                t = t + string.Format(@"<td>{0}</td>", Environment.ProcessorCount);
                t = t + string.Format(@"<td>{0} GB</td>", TotalMem / 1024 / 1024 / 1024);
                t = t + string.Format(@"<td>{0}%</td>", MemPercent);
                t = t + @"</tr>";
                t = t + @"</tbody>";
                t = t + @"</table>";
                t = t + @"</div>";

                t = t + @"<div class='content'>";
                t = t + @"<table class='DiskDrives'>";
                t = t + @"<thead>";
                t = t + @"<tr>";
                t = t + @"<th>Drive</th>";
                t = t + @"<th>Type</th>";
                t = t + @"<th>Size</th>";
                t = t + @"<th>Usage</th>";
                t = t + @"</tr>";
                t = t + @"</thead>";
                t = t + @"<tbody>";
                foreach (var drive in DriveInfo.GetDrives().Where(i => i.IsReady == true && i.DriveType != DriveType.CDRom))
                {
                    int percent = (int)(((double)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize) * 100);
                    t = t + string.Format(@"<tr class='{1}' id='{0}'>", drive.Name, percent >= 95 ? "Critical" : percent >= 85 ? "Warning" : "Good");
                    t = t + string.Format(@"<td>{0}</td>", drive.Name);
                    t = t + string.Format(@"<td>{0}</td>", drive.DriveType);
                    t = t + string.Format(@"<td>{0} GB</td>", drive.TotalSize / 1024 / 1024 / 1024);
                    t = t + string.Format(@"<td>{0}%</td>", percent);
                    t = t + @"</tr>";
                }
                t = t + @"</tbody>";
                t = t + @"</table>";
                t = t + @"</div>";

                t = t + @"</div>";
                t = t + @"</div>";
                t = t + @"</div>";
                return getHeader() + t + getFooter();
            };

            //Displays a list of all plugins registered
            Get["/plugin"] = x =>
            {
                if (Request.Query["json"].HasValue)
                {
                    var responce = (Response)JsonConvert.SerializeObject(pluginManager.AboutAll());
                    responce.ContentType = "application/json";
                    return responce;
                }
                else
                {
                    string t = "<link rel='stylesheet' href='/css/table.css' />";
                    t = t + "<link rel='stylesheet' href='/css/plugin.css' />";
                    t = t + @"<table class='blueTable'>";
                    t = t + @"<thead>";
                    t = t + @"<tr>";
                    t = t + @"<th>dllName</th>";
                    t = t + @"<th>className</th>";
                    t = t + @"<th>Version</th>";
                    t = t + @"</tr>";
                    t = t + @"</thead>";
                    t = t + @"<tbody>";
                    foreach (BatMonPluginAbout a in pluginManager.AboutAll())
                    {
                        t = t + @"<tr>";
                        t = t + string.Format(@"<td>{0}</td>", a.dllName);
                        t = t + string.Format(@"<td><a href='/plugin/{1}'>{0}</a></td>", a.className, pluginManager[a.className].Metadata.Name);
                        t = t + string.Format(@"<td>{0}</td>", a.Version);
                        t = t + @"</tr>";
                    }
                    t = t + @"</tbody>";
                    t = t + @"</table>";

                    return getHeader() + t + getFooter();
                }
            };

            //BatMonPlugin Dynamically added addresses
            if (!(pluginManager.Plugins is null))
            { 
                foreach (Lazy<IBatMonPlugin, IMetadata> p in pluginManager.Plugins)
                {
                    Get[string.Format("/Plugin/{0}", p.Metadata.Name)] = x =>
                     {
                         if (Request.Query["json"].HasValue)
                         {
                             p.Value.Run();
                             var responce = (Response)JsonConvert.SerializeObject(p.Value.Results.Values);
                             responce.ContentType = "application/json";
                             return responce;
                         }
                         else if (Request.Query["appd"].HasValue)
                         {
                             p.Value.Run();
                             var responce = (Response)p.Value.Results.ToAppD();
                             responce.ContentType = "application/json";
                             return responce;
                         }
                         else
                         {
                             p.Value.Run();
                             string t = "<link rel='stylesheet' href='/css/table.css' />";
                             t = t + @"<table class='blueTable'>";
                             t = t + @"<thead>";
                             t = t + @"<tr>";
                             foreach (var field in p.Value.Results.Fields)
                             {
                                 t = t + string.Format(@"<th>{0}</th>", field.Label);
                             }
                             t = t + @"</tr>";
                             t = t + @"</thead>";
                             t = t + @"<tbody>";
                             foreach (var value in p.Value.Results.Values)
                             {
                                 t = t + string.Format(@"<tr class='{0}'>", value.ErrorCode);
                                 t = t + string.Format(@"<td>{0}</td>", value.ApplicationName);
                                 t = t + string.Format(@"<td>{0}</td>", value.TierName);
                                 t = t + string.Format(@"<td>{0}</td>", value.ProcessName);
                                 t = t + string.Format(@"<td>{0}</td>", value.ErrorCode);
                                 t = t + string.Format(@"<td>{0}</td>", value.ErrorDescription);
                                 t = t + @"</tr>";
                             }
                             t = t + @"</tbody>";
                             t = t + @"</table>";

                             return getHeader() + t + getFooter();
                         }
                     };
                }
            }
        }
    }
}
