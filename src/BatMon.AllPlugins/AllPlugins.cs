/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using BatMon.Framework;
using System.ComponentModel.Composition;
using System.Linq;

namespace BatMon.AllPlugins
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "AllPlugins")]
    [ExportMetadata("isAggregate", true)]
    public class AllPlugins : BatMonPlugin, IBatMonPlugin
    {

        public override MonitorResults Results
        {
            get
            {       
                MonitorResults r = new MonitorResults();
                foreach (var plugin in BatMonPluginManager.getCurrentInstance.Plugins.Where(p => p.Metadata.isAggregate == false).Where(p => p.Value.Results!=null && p.Value.Results.Values.Count > 0 ))
                {
                    r.Values.AddRange(plugin.Value.Results.Values);
                }
                return r;
            }
        }

        public new bool Run()
        {
            return true;
        }
    }
}
