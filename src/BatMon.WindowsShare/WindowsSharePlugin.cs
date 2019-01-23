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
                Match remoteHost = new Regex(@"(?<=^\\\\)[^\\]*?(?=\\|$)").Match(f.Path);
                CredentialCache loginCache = new CredentialCache();
                //Create credentials if needed
                if (f.User != null)
                {
                    NetworkCredential login = new NetworkCredential(f.User, f.Password);
                    loginCache.Add(new System.Uri(string.Format(@"\\{0}", remoteHost)), "Negotiate", login);
                }

                //Run Filesystem Test
                if (!System.IO.Directory.Exists(f.Path)) { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Error", string.Format("Path could not be found ", f.Path))); }
                else
                {
                    if (System.IO.Directory.GetFiles(f.Path).Length <= 0) { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Error", string.Format("Path contains 0 files ", f.Path))); }
                    else { r.Add(new Result(f.ApplicationName, f.TierName, f.ProcessName, "Successful", string.Format("Path could not be found ", f.Path))); }
                }

                

                //Clean up credentials created
                if (f.User != null) { loginCache.Remove(new System.Uri(string.Format(@"\\{0}", remoteHost)), "Negotiate"); }
                loginCache = null;
            }
            return r.ToArray();
        }
    }
}
