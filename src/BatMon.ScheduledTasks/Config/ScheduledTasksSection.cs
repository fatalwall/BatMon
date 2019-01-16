/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.Configuration;

namespace BatMon.ScheduledTasks.Config
{
    public class ScheduledTasksSection : ConfigurationSection
    {
        [ConfigurationProperty("ResultCodes", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ResultCodeCollection),
            AddItemName = "add",
            ClearItemsName = "Clear",
            RemoveItemName = "Remove")]
        public ResultCodeCollection ResultCodes
        { 
            get { return ((ResultCodeCollection)base["ResultCodes"]) ?? new ResultCodeCollection(); }
        }

        [ConfigurationProperty("FolderFilters", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FolderFilterCollection),
            AddItemName = "add",
            ClearItemsName = "Clear",
            RemoveItemName = "Remove")]
        public FolderFilterCollection FolderFilters
        {
            get { return ((FolderFilterCollection)base["FolderFilters"]) ?? new FolderFilterCollection(); }
        }

        [ConfigurationProperty("Application")]
        public ApplicationElement Application
        { get { return (ApplicationElement)base["Application"]; } }

        [ConfigurationProperty("Tier")]
        public TierElement Tier
        { get { return (TierElement)base["Tier"]; } }

        [ConfigurationProperty("ApplicationDynamicOverride")]
        public ApplicationDynamicOverrideElement ApplicationDynamicOverride
        { get { return (ApplicationDynamicOverrideElement)base["ApplicationDynamicOverride"]; } }

        [ConfigurationProperty("TierDynamicOverride")]
        public TierDynamicOverrideElement TierDynamicOverride
        { get { return (TierDynamicOverrideElement)base["TierDynamicOverride"]; } }

        [ConfigurationProperty("InitialStageResultCodes")]
        public InitialStageResultCodesElement InitialStageResultCodes
        { get { return ((InitialStageResultCodesElement)base["InitialStageResultCodes"]) ?? new InitialStageResultCodesElement(false); } }
    }
}