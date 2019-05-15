using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BatMon.UriMonitor.Config
{
    public class EndPoints : ConfigurationElementCollection
    {
        public EndPoint this[int index] { get { return (EndPoint)BaseGet(index); } }
        public int IndexOf(EndPoint e) { return BaseIndexOf(e); }
        public override System.Configuration.ConfigurationElementCollectionType CollectionType { get { return System.Configuration.ConfigurationElementCollectionType.BasicMap; } }

        protected override ConfigurationElement CreateNewElement() { return new EndPoint(); }

        protected override object GetElementKey(ConfigurationElement element) { return ((EndPoint)element); }

        public void Add(EndPoint e) { BaseAdd(e); }
        public void Remove(EndPoint e) { if (BaseIndexOf(e) >= 0) BaseRemove(e); }
        public void RemoveAt(int index) { BaseRemoveAt(index); }

        public void Clear() { BaseClear(); }

        protected override string ElementName { get { return "EndPoint"; } }
    }
}
