
namespace BatMon
{
    class BatMonService
    {
        public bool Start()
        {
            var t = BatMonPluginManager.getCurrentInstance;
            t.DoImport();
            return true;
        }

        public bool Stop()
        {
            return true;
        }
    }
}
