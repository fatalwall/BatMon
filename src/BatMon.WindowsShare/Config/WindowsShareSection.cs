using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;

namespace BatMon.WindowsShare.Config
{
    public class WindowsShareSection : ConfigurationSection
    {
        private static readonly WindowsShareSection instance = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections.OfType<WindowsShareSection>().FirstOrDefault() as WindowsShareSection 
                                                            ?? ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location).Sections.OfType<WindowsShareSection>().FirstOrDefault() as WindowsShareSection;

        public static WindowsShareSection getCurrentInstance { get { return instance; } }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public FolderCollection Folders
        {
            get { return ((FolderCollection)base[""]) ?? new FolderCollection(); }
        }
    }
}
