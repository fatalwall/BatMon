/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using Newtonsoft.Json;

namespace BatMon.Framework
{
    public class Field
    {
        public Field(string Label)
        {
            this.Label = Label;
        }
        public string Label { get; set; }
        public string ToAppD()
        {
            return string.Format("{{\"label\": {0}}}", JsonConvert.SerializeObject(this.Label));
        }
    }
}
