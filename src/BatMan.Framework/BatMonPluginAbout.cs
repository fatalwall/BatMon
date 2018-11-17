/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

namespace BatMon.Framework
{
    public class BatMonPluginAbout
    {
        public BatMonPluginAbout(string dllName, string className, System.Version version)
        {
            this.dllName = dllName;
            this.className = className;
            this.Version = version;
        }
        public string className { get; private set; }

        public string dllName { get; private set; }

        public System.Version Version { get; private set; }

        public override string ToString() { return string.Format("{0}.{1} ({2})", this.dllName,this.className, this.Version); }

    }
}
