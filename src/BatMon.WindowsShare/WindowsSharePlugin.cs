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

namespace BatMon.WindowsShare
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "WindowsShare")]
    public class WindowsSharePlugin : BatMonPlugin, IBatMonPlugin
    {

        protected override Result[] fetchResults()
        {
            return null;
        }
    }
}
