/* 
 *Copyright (C) 2019 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using BatMon.Framework;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace BatMon.ActiveConnections
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "ActiveConnections")]
    [ExportMetadata("isAggregate", false)]
    public class ActiveConnectionsPlugin : BatMonPlugin, IBatMonPlugin
    {
        protected override Result[] fetchResults()
        {
            IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();

            return null;
        }
    }
}
