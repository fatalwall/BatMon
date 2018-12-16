/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

namespace BatMon.Framework
{
    public interface IBatMonPlugin
    {           
        MonitorResults Results { get; }
        
        BatMonPluginAbout About { get; }

        bool Run();

        string htmlPieChart();

        string htmlWidget();

    }

    public interface IMetadata
    {
        string Name { get; }
        bool isAggregate { get; }
    }
}
