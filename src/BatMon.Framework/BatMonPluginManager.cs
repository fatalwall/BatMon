/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Linq;
using BatMon.Framework;
using System.Configuration;
using BatMon.Framework.Config;
using NLog;
using System.Reflection;

namespace BatMon
{
    public sealed class BatMonPluginManager
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly BatMonPluginManager instance = new BatMonPluginManager();
        public static BatMonPluginManager getCurrentInstance
        {
            get
            {
                return instance;
            }
        }

        private static readonly BatMonSection settings = (ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections.OfType<BatMonSection>().FirstOrDefault() as BatMonSection) ?? new BatMonSection();
        public static BatMonSection Settings
        {
            get
            {
                return settings;
            }
        }

        [ImportMany]
        private IEnumerable<Lazy<IBatMonPlugin, IMetadata>> modules;
        public IEnumerable<Lazy<IBatMonPlugin, IMetadata>> Plugins { get { return modules?.OrderBy(m => m.Value.About.dllName + "." + m.Value.About.className) ?? null; } }

        public Lazy<IBatMonPlugin, IMetadata> this[string NameOrClass]
        {
            get
            {
                foreach (Lazy<IBatMonPlugin, IMetadata> p in Plugins)
                {
                    if (p.Metadata.Name == NameOrClass || p.Value.About.className == NameOrClass)
                        return p;
                }
                return null;
            }
        }

        public BatMonPluginManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void DoImport()
        {
            var catalog = new AggregateCatalog();

            //Load from executing assemblies plugin subdirectory
            try
            {
                //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)   
                catalog.Catalogs.Add(new DirectoryCatalog(@".\Plugins"));
                foreach (var path in System.IO.Directory.EnumerateDirectories(@".\Plugins", "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(path));
                }              
            }
            catch
            {
                catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));
                foreach (var path in System.IO.Directory.EnumerateDirectories(AppDomain.CurrentDomain.BaseDirectory, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(path));
                }
            }
            CompositionContainer container = new CompositionContainer(catalog);

            try { container.ComposeParts(this); }
            catch(System.Reflection.ReflectionTypeLoadException) { }//if no modules load oh well
        }

        public int Count
        {
            get { return modules != null ? modules.Count() : 0; }
        }

        public BatMonPluginAbout[] AboutAll()
        {
            var result = new List<BatMonPluginAbout>();
            if (!(modules is null))
            foreach (Lazy<IBatMonPlugin, IMetadata> com in Plugins)
            {
                result.Add(com.Value.About);
            }
            return result.ToArray();
        }

        public List<Result> RunAll()
        {
            var result = new List<Result>();
            if (!(modules is null))
            foreach (Lazy<IBatMonPlugin, IMetadata> com in Plugins.Where(p => !p.Metadata.isAggregate))
            {
                if (com.Value.Run())
                { result.AddRange(com.Value.Results.Values); }
            }

            return result;
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly a = null;

            //App Folder
            string exePath = System.IO.Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            if (a is null) { a = isAssemblyHere(exePath, args); }
            //App/Plugins Folder
            string pluginsPath = System.IO.Path.Combine(exePath, "Plugins");
            if (a is null) { a = isAssemblyHere(pluginsPath, args); }
            //App/Plugins/PluginName Folder
            string pluginPath = System.IO.Path.Combine(pluginsPath, args.Name);
            if (a is null) { a = isAssemblyHere(pluginPath, args); }
            //System32 Folder
            if (a is null) { a = isAssemblyHere(Environment.SystemDirectory, args); }
            //Windows Folder
            if (a is null) { a = isAssemblyHere(Environment.GetFolderPath(Environment.SpecialFolder.Windows), args); }
            if (a is null) { LogManager.GetCurrentClassLogger().Error("{0} could not be found in any of the following locations:\r\n\t{1}\r\n\t{2}\r\n\t{3}\r\n\t{4}\r\n\t{5}\r\n", args.Name, exePath, pluginsPath, pluginPath, Environment.SystemDirectory, Environment.GetFolderPath(Environment.SpecialFolder.Windows)); }
            return a;
        }
        private static Assembly isAssemblyHere(string directory, ResolveEventArgs args)
        {
            string path = Path.Combine(directory, string.Format("{0}.dll", args.Name));
            try { return File.Exists(path) ? Assembly.LoadFile(path) : null; }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, string.Format("Failed to load assembly where it was found: {0}", path));
                return null;
            }
        }
    }
}
