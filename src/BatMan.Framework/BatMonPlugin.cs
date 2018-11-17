/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;

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


        public MonitorResults Results { get; set; }

        public BatMonPluginAbout About { get { return new BatMonPluginAbout(this.GetType().Assembly.GetName().Name, this.GetType().Name,this.GetType().Assembly.GetName().Version); } }

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
                Results.Values.AddRange(getResults());
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
        protected virtual Result[] getResults()
        {
            throw new NotImplementedException("getResults has not been implemented");
        }
    }
}
