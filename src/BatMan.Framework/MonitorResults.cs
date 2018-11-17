/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.Collections.Generic;
using System.Linq;

namespace BatMon.Framework
{
    public class MonitorResults
    {
        public MonitorResults()
        {
            Fields = new List<Field>();
            Fields.Add(new Field("Application Name"));
            Fields.Add(new Field("Tier Name"));
            Fields.Add(new Field("Process Name"));
            Fields.Add(new Field("Error Code"));
            Fields.Add(new Field("Error Description"));

            Values = new List<Result>();

        }
        public List<Field> Fields { get; private set; }
        public List<Result> Values { get; private set; }

        public string ToAppD()
        {
            string outputField = "\"fields\": [";
            outputField += string.Join(",", Fields.Select(x => x.ToAppD()).ToArray());
            outputField += "]";

            string outputResult = "\"results\": [";
            outputResult += string.Join(",", Values.Select(x => x.ToAppD()).ToArray());
            outputResult += "]";

            return string.Format("[{{{0},{1}}}]", outputField, outputResult);
        }
    }
}
