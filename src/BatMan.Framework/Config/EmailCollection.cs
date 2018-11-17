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

namespace BatMon.Framework.Config
{
    public class EmailCollection : ConfigurationElementCollection
    {

        #region Generic Minor Edits
        public EmailCollection()
        {
            EmailElement e = (EmailElement)CreateNewElement();
            if (e.onErrorCode != "")
            {
                Add(e);
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new EmailElement();
        }
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((EmailElement)element).onErrorCode;
        }
        public EmailElement this[int index]
        {
            get
            {
                return (EmailElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
        new public EmailElement this[string Name]
        {
            get
            {
                return (EmailElement)BaseGet(Name);
            }
        }
        public int IndexOf(EmailElement e)
        {
            return BaseIndexOf(e);
        }
        public int IndexOf(String Name)
        {
            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].onErrorCode.ToUpper() == Name.ToUpper())
                    return idx;
            }
            return -1;
        }
        public void Add(EmailElement e)
        {
            BaseAdd(e);
        }
        public void Remove(EmailElement e)
        {
            if (BaseIndexOf(e) >= 0)
                BaseRemove(e.onErrorCode);
        }
        protected override string ElementName
        {
            get { return "Email"; }
        }
        #endregion

        #region Generic No Edit
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
        public void Remove(string name)
        {
            BaseRemove(name);
        }
        public void Clear()
        {
            BaseClear();
        }
    #endregion

    }
}
