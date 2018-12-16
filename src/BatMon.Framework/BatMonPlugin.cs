/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace BatMon.Framework
{
    public class BatMonPlugin : IBatMonPlugin
    {
        private NLog.Logger _logger;
        protected NLog.Logger logger
        {
            get
            {
                if (_logger is null) _logger = NLog.LogManager.GetLogger(GetType().FullName);
                return _logger;
            }
        }


        public virtual MonitorResults Results { get; private set; }

        public BatMonPluginAbout About { get { return new BatMonPluginAbout(this.GetType().Assembly.GetName().Name, this.GetType().Name, this.GetType().Assembly.GetName().Version); } }

        /// <summary>
        /// Implemetation of Plugin initiator that kicks off overidable method getResults() with 
        /// default error handling to insulate the plugin manager from uncaught exceptions within 
        /// plugins.
        /// </summary>
        /// <returns>boolean value indicating if the plugin succeded or failed to run</returns>
        public bool Run()
        {
            try
            {
                Results = new MonitorResults();
                Results.Values.AddRange(fetchResults());
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Overridable method that returns a result set back to the Run method.
        /// </summary>
        /// <returns></returns>
        protected virtual Result[] fetchResults()
        {
            throw new NotImplementedException("fetchResults has not been implemented");
        }

        /// <summary>
        /// Implemetation of standard access method to Metadata Attributes
        /// </summary>
        /// <returns>Object value</returns>
        protected object MetadataAttribute(string Name)
        {
            foreach (var a in this.GetType().GetCustomAttributes(typeof(ExportMetadataAttribute), true).OfType<ExportMetadataAttribute>().Where(a => a.Name == Name))
            {
                return a.Value;
            }
            return null;
        }

        /// <summary>
        /// Implemetation of standard pie chart for web page
        /// </summary>
        /// <returns>string value containing needed javascript, html, css, etc.</returns>
        public string htmlPieChart()
        {
            string output = "";
            output = output + string.Format(@"google.charts.setOnLoadCallback(drawChart{0});", this.MetadataAttribute("Name").ToString());
            output = output + string.Format("function drawChart{0}() {{ var data = google.visualization.arrayToDataTable([", this.MetadataAttribute("Name").ToString());
            output = output + string.Format("['{0}', '{1}']", "ErrorCode", "Counts");

            foreach (var g in this.Results.Values.GroupBy(i => i.ErrorCode))
            {
                output = output + string.Format(",['{0}', {1}]", g.Key, g.Count());
            }
            output = output + string.Format(@"]); var options = {{ pieSliceText: 'label', 'legend':'none', 'width':'100%', 'height':'100%', title: '{0}', titleTextStyle: {{ 'fontSize':14, 'bold':true }}  }};", this.MetadataAttribute("Name").ToString());
            output = output + string.Format(@"var chart = new google.visualization.PieChart(document.getElementById('{0}'));", this.MetadataAttribute("Name").ToString());
            output = output + string.Format(@"chart.draw(data, options);}}", this.MetadataAttribute("Name").ToString());
            return output;
        }

        public string htmlWidget()
        { return ""; }
    }
}
