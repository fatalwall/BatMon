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
using BatMon.WindowsShare.Config;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using vshed.IO;

namespace BatMon.WindowsShare
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "WindowsShare")]
    [ExportMetadata("isAggregate", false)]
    public class WindowsSharePlugin : BatMonPlugin, IBatMonPlugin
    {
        protected override Result[] fetchResults()
        {
            List<Result> r = new List<Result>();
            WindowsShareSection settings = WindowsShareSection.getCurrentInstance;
            foreach (FolderElement f in settings.Folders)
            {
                logger.Trace(string.Format("Path: {0} User: {1}", f.Path, f.User??""));

                UncShare Credentials = null;
                try
                {
                    //Initialize Credentials if needed
                    if (f.User != null)
                        Credentials = new UncShare(f.Path, f.User, f.Password);

                    //Run Filesystem Test
                    if (!System.IO.Directory.Exists(f.Path)) { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Failure", string.Format("Path could not be found {0}", f.Path))); }
                    else
                    {

                        if (f.ContentCheck & System.IO.Directory.GetFiles(f.Path).Length <= 0 & System.IO.Directory.GetDirectories(f.Path).Length <= 0) { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Error", string.Format("Path contains 0 files and directories {0}", f.Path))); }
                        else { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Successful", "Folder or Network share is acessable")); }
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Failure", ex.Message));
                }
                finally
                {
                    //Clean up credentials if needed
                    if (Credentials != null)
                        Credentials.Dispose();
                }

            }
            return r.ToArray();
        }
    }
}
