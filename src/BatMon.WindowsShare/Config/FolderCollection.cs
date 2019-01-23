using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BatMon.WindowsShare.Config
{
    public class FolderCollection : ConfigurationElementCollection
    {
        public FolderElement this[int index] { get { return (FolderElement)BaseGet(index); } }
        public new FolderElement this[string Path] { get { return this.Cast<FolderElement>().Where(f => f.Path == Path).First(); } }
        public int IndexOf(FolderElement e) { return BaseIndexOf(e); }
        public override System.Configuration.ConfigurationElementCollectionType CollectionType { get { return System.Configuration.ConfigurationElementCollectionType.BasicMap; } }

        protected override ConfigurationElement CreateNewElement() { return new FolderElement(); }

        protected override object GetElementKey(ConfigurationElement element) { return ((FolderElement)element).Path; }

        public void Add(FolderElement e) { BaseAdd(e); }
        public void Remove(FolderElement e) { if (BaseIndexOf(e) >= 0) BaseRemove(e.Path); }
        public void RemoveAt(int index) { BaseRemoveAt(index); }

        public void Clear() { BaseClear(); }

        protected override string ElementName { get { return "Folder"; } }
    }
}
