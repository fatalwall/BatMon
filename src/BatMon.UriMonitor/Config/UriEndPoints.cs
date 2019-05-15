using System.Linq;
using System.Configuration;
using System.Reflection;

namespace BatMon.UriMonitor.Config
{
    public class UriEndPoints : ConfigurationSection
    {
        private static readonly UriEndPoints instance = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections.OfType<UriEndPoints>().FirstOrDefault() as UriEndPoints
                                                    ?? ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location).Sections.OfType<UriEndPoints>().FirstOrDefault() as UriEndPoints;

        public static UriEndPoints getCurrentInstance { get { return instance; } }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EndPoints EndPoints
        {
            get { return ((EndPoints)base[""]) ?? new EndPoints(); }
        }
    }
}
