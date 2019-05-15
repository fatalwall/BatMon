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

namespace BatMon.UriMonitor
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "UriMonitor")]
    [ExportMetadata("isAggregate", false)]
    public class UriPlugin : BatMonPlugin, IBatMonPlugin
    {
        protected override Result[] fetchResults()
        {
            List<Result> r = new List<Result>();
            foreach (Config.EndPoint s in Config.UriEndPoints.getCurrentInstance.EndPoints)
            {
                try
                {
                    r.Add(new Result(s.ApplicationName, s.TierName, s.ProcessName, s.TestResult() ? "Success" : "Error", s.TestResultMessage, s.Result));
                } catch(System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }
            return r.ToArray();
        }
    }
}
