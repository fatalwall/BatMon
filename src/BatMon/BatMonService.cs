using NLog;
using System.Text;

namespace BatMon
{
    class BatMonService
    {
        Logger logger = LogManager.GetCurrentClassLogger();        
        public bool Start()
        {
            var t = BatMonPluginManager.getCurrentInstance;
            t.DoImport();
            StringBuilder pList = new StringBuilder();
            pList.AppendLine("BatMon is starting with the following plugins:");
            if (t.Plugins is null)
            {
                pList.AppendLine("  No plugins loaded");
            }
            else
            {
                foreach (var a in t.Plugins)
                {
                    pList.AppendFormat("  - {0}", a.Metadata.Name).AppendLine();
                }
            }
            logger.Info(pList);
            return true;
        }

        public bool Stop()
        {
            return true;
        }
    }
}
