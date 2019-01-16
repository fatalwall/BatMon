/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using BatMon.Framework;
using BatMon.ScheduledTasks.Config;
using Microsoft.Win32.TaskScheduler;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.IO;

namespace BatMon.ScheduledTasks
{
    [Export(typeof(IBatMonPlugin))]
    [ExportMetadata("Name", "ScheduledTasks")]
    [ExportMetadata("isAggregate", false)]
    public class ScheduledTasksPlugin : BatMonPlugin, IBatMonPlugin
    {
        private string DetermineApplication(Task task, ScheduledTasksSection settings)
        {
            string Application = settings.Application.Value;
            if (settings.ApplicationDynamicOverride != null & !String.IsNullOrEmpty(settings.ApplicationDynamicOverride.Value))
            {
                Match m = Regex.Match(task.Folder.Path, settings.ApplicationDynamicOverride.Value);
                if (m.Success)
                { Application = string.IsNullOrWhiteSpace(m.Groups["Application"].Value) ? Application : m.Groups["Application"].Value; }
                else
                {
                    m = Regex.Match(task.Path, settings.ApplicationDynamicOverride.Value);
                    if (m.Success)
                    { Application = string.IsNullOrWhiteSpace(m.Groups["Application"].Value) ? Application : m.Groups["Application"].Value; }
                }
            }
            return Application;
        }
        private string DetermineTier(Task task, ScheduledTasksSection settings)
        {
            string Tier = settings.Tier.Value;
            if (settings.TierDynamicOverride != null & !String.IsNullOrEmpty(settings.TierDynamicOverride.Value))
            {
                Match m = Regex.Match(task.Folder.Path, settings.TierDynamicOverride.Value);
                if (m.Success)
                { Tier = string.IsNullOrWhiteSpace(m.Groups["Tier"].Value) ? Tier : m.Groups["Tier"].Value; }
                else
                {
                    m = Regex.Match(task.Path, settings.TierDynamicOverride.Value);
                    if (m.Success)
                    { Tier = string.IsNullOrWhiteSpace(m.Groups["Tier"].Value) ? Tier : m.Groups["Tier"].Value; }
                }
            }
            return Tier;
        }
        protected Result[] InitialStageResultCodesResults(ScheduledTasksSection settings)
        {
            List<Result> r = new List<Result>();
            using (TaskService ts = new TaskService())
            {
                foreach (Task t in ts.AllTasks)
                {
                    if (!settings.FolderFilters.Match(t.Folder.Path))
                    {
                        //Determine Application and Tier
                        string Application = DetermineApplication(t, settings);
                        string Tier = DetermineTier(t, settings);

                        //Loop to add all potential result codes
                        foreach (ResultCodeElement rc in settings.ResultCodes)
                        {
                            r.Add(new Result(Application, Tier, t.Name, rc.AppDynamicsCode, rc.Description));
                        }
                    }
                }
            }
            return r.ToArray();
        }

        protected override Result[] fetchResults()
        {
            //create a fallback if no section exists in the app.config so that it will look for a ScheduledTasks.config
            ScheduledTasksSection settings;
            try
            { settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections.OfType<ScheduledTasksSection>().FirstOrDefault() as ScheduledTasksSection ?? ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location).Sections.OfType<ScheduledTasksSection>().FirstOrDefault() as ScheduledTasksSection; }
            catch (ConfigurationException cEx)
            {
                logger.Error(cEx, string.Format("Unable to load configuration {0}", Assembly.GetExecutingAssembly().Location + ".config"));
                settings = new ScheduledTasksSection();
            }
            if (settings is null)
            {
                logger.Error(string.Format("Unable to load configuration {0}", Assembly.GetExecutingAssembly().Location + ".config"));
                settings = new ScheduledTasksSection();
            }
            logger.Trace(string.Format("ScheduledTasks Configuration loaded with {0} Result Codes and {1} Folder Filters", settings.ResultCodes.Count(), settings.FolderFilters.Count()));


            if (settings.InitialStageResultCodes.Enabled == true)
            {
                //If true output will be an initial load of all possible outputs. This is to help populate all cases within app dynamics
                return InitialStageResultCodesResults(settings);
            }

            List<Result> r = new List<Result>();
            using (TaskService ts = new TaskService())
            {
                logger.Trace(string.Format("ScheduledTasks has detected {0} Tasks", ts.AllTasks.Count()));
                //Initialize logging
                foreach (Task t in ts.AllTasks)
                {
                    if (!settings.FolderFilters.Match(t.Folder.Path))
                    {
                        //Determine Application and Tier
                        string Application = DetermineApplication(t, settings);
                        string Tier = DetermineTier(t, settings);

                        //Determine Job state and add result
                        if (t.Enabled == true)
                        {
                            if (t.LastRunTime == DateTime.MinValue)
                            {
                                //Task has never run
                                r.Add(new Result(Application, Tier, t.Name, settings.ResultCodes["NeverRun"].AppDynamicsCode, settings.ResultCodes["NeverRun"].Description,t));
                                logger.Info(string.Format("Application: {0,-40}Tier: {1,-40}ProcessName: {2,-40}Exit Code: {3,-15}Error Code: {4,-20}Error Description: {5}"
                                                            , Application
                                                            , Tier
                                                            , t.Name.Substring(0, Math.Min(39, t.Name.Length))
                                                            , "NeverRun"
                                                            , settings.ResultCodes["NeverRun"].AppDynamicsCode
                                                            , settings.ResultCodes["NeverRun"].Description
                                                            )
                                );
                            }
                            else
                            {
                                //Base code off of the exit code
                                r.Add(new Result(Application, Tier, t.Name, settings.ResultCodes[t.LastTaskResult.ToString()].AppDynamicsCode, settings.ResultCodes[t.LastTaskResult.ToString()].Description, t));
                                if (settings.ResultCodes[t.LastTaskResult.ToString()] == settings.ResultCodes.Default)
                                {
                                    logger.Warn(string.Format("Application: {0,-40}Tier: {1,-40}ProcessName: {2,-40}Exit Code: {3,-15}Error Code: {4,-20}Error Description: {5}"
                                                                , Application
                                                                , Tier
                                                                , t.Name.Substring(0, Math.Min(39, t.Name.Length))
                                                                , t.LastTaskResult.ToString()
                                                                , settings.ResultCodes[t.LastTaskResult.ToString()].AppDynamicsCode
                                                                , settings.ResultCodes[t.LastTaskResult.ToString()].Description
                                                                )
                                    );
                                }
                                else
                                {
                                    logger.Info(string.Format("Application: {0,-40}Tier: {1,-40}ProcessName: {2,-40}Exit Code: {3,-15}Error Code: {4,-20}Error Description: {5}"
                                                                , Application
                                                                , Tier
                                                                , t.Name.Substring(0, Math.Min(39, t.Name.Length))
                                                                , t.LastTaskResult.ToString()
                                                                , settings.ResultCodes[t.LastTaskResult.ToString()].AppDynamicsCode
                                                                , settings.ResultCodes[t.LastTaskResult.ToString()].Description
                                                                )
                                    );
                                }
                            }
                        }
                        else
                        {
                            //Job is disabled
                            r.Add(new Result(Application, Tier, t.Name, settings.ResultCodes["Disabled"].AppDynamicsCode, settings.ResultCodes["Disabled"].Description,t));
                            logger.Info(string.Format("Application: {0,-40}Tier: {1,-40}ProcessName: {2,-40}Exit Code: {3,-15}Error Code: {4,-20}Error Description: {5}"
                                                            , Application
                                                            , Tier
                                                            , t.Name.Substring(0, Math.Min(39, t.Name.Length))
                                                            , "Disabled"
                                                            , settings.ResultCodes["Disabled"].AppDynamicsCode
                                                            , settings.ResultCodes["Disabled"].Description
                                                            )
                                );
                        }
                    }
                }
            }
            return r.ToArray();
        }
    }
}
