/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.Configuration;

namespace BatMon.ScheduledTasks.Config
{
    public class ResultCodeCollection : ConfigurationElementCollection
    {
        public int IndexOf(String ExitCode)
        {
            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].ExitCode.ToUpper() == ExitCode.ToUpper())
                    return idx;
            }
            return -1;
        }

        public ResultCodeElement this[int index]
        {
            get { return (ResultCodeElement)BaseGet(index); }
        }

        public new ResultCodeElement this[String ExitCode]
        {
            get
            {
                if (IndexOf(ExitCode) < 0)
                {
                    return this.Default;
                }
                return (ResultCodeElement)BaseGet(ExitCode);
            }
        }

        public ResultCodeElement Default
        {
            get {
                if (IndexOf("") < 0) return null;
                return (ResultCodeElement)BaseGet("");
            }
        }

        public void Add(ResultCodeElement c) { BaseAdd(c); }

        public void Remove(ResultCodeElement c) { if (BaseIndexOf(c) >= 0) BaseRemove(c.ExitCode); }

        public void RemoveAt(int index) { BaseRemoveAt(index); }

        public void Remove(String ID) { BaseRemove(ID); }

        public void Clear() { BaseClear(); }

        public new int Count() { return base.Count; }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ResultCodeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ResultCodeElement)element).ExitCode;
        }

        protected override string ElementName
        {
            get { return "ResultCode"; }
        }
    }
}