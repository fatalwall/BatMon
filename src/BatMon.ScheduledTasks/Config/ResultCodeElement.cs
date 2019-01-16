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

namespace BatMon.ScheduledTasks.Config
{
    public class ResultCodeElement : ConfigurationElement
    {
        public ResultCodeElement() {}
        public ResultCodeElement(String ExitCode, String AppDynamicsCode, String Description)
        {
            this.ExitCode = ExitCode;
            this.AppDynamicsCode = AppDynamicsCode;
            this.Description = Description;
        }

        [ConfigurationProperty("ExitCode", IsKey = true, IsRequired = false)]
        public String ExitCode {
            get { return base["ExitCode"] as string; }
            set { base["ExitCode"] = value; }
        }

        [ConfigurationProperty("AppDynamicsCode", IsKey = false, IsRequired = false)]
        public String AppDynamicsCode
        {
            get { return base["AppDynamicsCode"] as string; }
            set { base["AppDynamicsCode"] = value; }
        }

        [ConfigurationProperty("Description", IsKey = false, IsRequired = false)]
        public String Description
        {
            get { return base["Description"] as string; }
            set { base["Description"] = value; }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            //get Attributes (ID and User)
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                try { this[reader.Name] = reader.Value; }
                catch (Exception e) { throw new ConfigurationErrorsException("XMLPart: ResultCodeElement" + ", Attribute: " + reader.Name + ", Error: Unable to read in Value", e); }
            }

            //Get Text Content (Password)
            reader.MoveToElement();
            this.Description = reader.ReadElementContentAsString();
        }

        public override bool Equals(object o)
        {
            if (o.GetType() == typeof(ResultCodeElement))
            { return (ResultCodeElement)o == this; }
            else { return false; } 
        }
        public override int GetHashCode()
        {
            return this.ExitCode.GetHashCode() * 17 + this.AppDynamicsCode.GetHashCode() * 17 + this.Description.GetHashCode();
        }

        public static bool operator == (ResultCodeElement object1, ResultCodeElement object2)
        {
            
            return ((object1.ExitCode == object2.ExitCode) && (object1.AppDynamicsCode == object2.AppDynamicsCode) && (object1.Description == object2.Description));
        }

        public static bool operator !=(ResultCodeElement object1, ResultCodeElement object2)
        {
            return !(object1 == object2);
        }
    }
}