/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.Linq;
using BatMon.Framework.Config;
using System.Configuration;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System;


namespace BatMon.Framework
{
    public class Result
    {
        public Result(string ApplicationName, string TierName, string ProcessName, string ErrorCode, string ErrorDescription, object obj = null)
        {
            this.Obj = obj;
            this.ApplicationName = ApplicationName;
            this.TierName = TierName;
            this.ProcessName = ProcessName;
            this.ErrorCode = ErrorCode;
            this.ErrorDescription = ExpandVariables(ErrorDescription);
        }

        public string ApplicationName { get; set; }
        public string TierName { get; set; }
        public string ProcessName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

        [JsonIgnore]
        public object Obj { get; private set; }

        public string ToAppD()
        {
            return string.Format("[{0},{1},{2},{3},{4}]", JsonConvert.SerializeObject(this.ApplicationName), JsonConvert.SerializeObject(this.TierName), JsonConvert.SerializeObject(this.ProcessName), JsonConvert.SerializeObject(this.ErrorCode), JsonConvert.SerializeObject(this.ErrorDescription));
        }

        public string ExpandVariables(string s)
        {
            if (s is null) { return null; }
            MatchCollection mc = Regex.Matches(s, @"\${(?'Object'.*?)?(?:\:(?'Item'.*?))?(?:\[(?'ItemSub1'.*?)\](?:\[(?'ItemSub2'.*?)\])?)?(?:->[fF][oO][rR][mM][aA][tT]=(?'Format'.*?))?}");
            /* Groups
             *   Object        - Returns the objects toString value
             *   Item       - Name of the Variable or Property you want
             *   ItemSub1   - (Optional) Sub property of Item (Row if a dataset)
             *   ItemSub2   - (Optional) for use with datasets as Column 
             *   Format     - (Optional - can be used even without any of the previous options set)
             *                  For Supported formating go to https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types
             *   
             * Example Matchs
             *   ${Obj}                                                         =   1
             *   ${Obj->format=yyyy-MM-dd HH:mm:ss.fff}                         =   3
             *   ${Obj:ExitCode}                                                =   5
             *   ${Obj:ExitCode->format=yyyy-MM-dd HH:mm:ss.fff}                =   7
             *   ${Obj[BaudRate]}                                               =   9
             *   ${Obj[BaudRate]->format=#.##}                                  =   11
             *   ${Obj:Port[BaudRate]}                                          =   13
             *   ${Obj:Port[BaudRate]->format=yyyy-MM-dd HH:mm:ss.fff}          =   15
             *   ${Obj[Row][Column]}                                            =   19
             *   ${Obj[Row][Column]->format=#.##}                               =   21
             *   ${Obj:DataSet[Row][Column]}                                    =   23
             *   ${Obj:DataSet[Row][Column]->format=yyyy-MM-dd HH:mm:ss.fff}    =   25
             * 
            */

            foreach (Match m in mc)
            {
                //Substitute the variable ${Obj} with its objects value
                s = s.Replace(m.Value, ConfigVariable_ToValue(m));
            }
            return Environment.ExpandEnvironmentVariables(s);
        }

        private string ConfigVariable_ToValue(Match m)
        {
            dynamic t_obj;
            dynamic t_item;
            dynamic t_itemSub1;
            dynamic t_itemSub2;
            string newValue= "[INVALID VARIABLE]";
            switch (ConfigVariable_PartScore(m))
            {
                case 1:
                    //Object = 1
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    newValue = t_obj?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 3:
                    //Object = 1
                    //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    try
                    { newValue = t_obj?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_obj?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                case 5: //Validated by Testing
                        //Object = 1
                        //Item = 4
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    newValue = t_item?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 7: //DateTime Validated by testing
                        //Object = 1
                        //Item = 4
                        //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    try
                    { newValue = t_item?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_item?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                case 9: // FIX ME
                        //Object = 1
                        //ItemSub1 = 8
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_itemSub1 = t_obj?[m.Groups["ItemSub1"].Value];
                    newValue = t_itemSub1?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 11: // FIX ME
                         //Object = 1
                         //ItemSub1 = 8
                         //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_itemSub1 = t_obj?[m.Groups["ItemSub1"].Value];
                    try
                    { newValue = t_itemSub1?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_itemSub1?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                case 13: //FIX ME
                         //Object = 1
                         //Item = 4
                         //ItemSub1 = 8
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    t_itemSub1 = t_item?[m.Groups["ItemSub1"].Value];
                    newValue = t_itemSub1?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 15: // FIX ME
                         //Object = 1
                         //Item = 4
                         //ItemSub1 = 8
                         //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    t_itemSub1 = t_item?[m.Groups["ItemSub1"].Value];
                    try
                    { newValue = t_itemSub1?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_itemSub1?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                case 19: // FIX ME
                         //Object = 1
                         //ItemSub1 = 8
                         //ItemSub2 =10
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_itemSub1 = t_obj?[m.Groups["ItemSub1"].Value];
                    t_itemSub2 = t_itemSub1?[m.Groups["ItemSub2"].Value];
                    newValue = t_itemSub2?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 21: // FIX ME
                         //Object = 1
                         //ItemSub1 = 8
                         //ItemSub2 =10
                         //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_itemSub1 = t_obj?[m.Groups["ItemSub1"].Value];
                    t_itemSub2 = t_itemSub1?[m.Groups["ItemSub2"].Value];
                    try
                    { newValue = t_itemSub2?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_itemSub2?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                case 23: // FIX ME
                         //Object = 1
                         //Item = 4
                         //ItemSub1 = 8
                         //ItemSub2 =10
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    t_itemSub1 = t_item?[m.Groups["ItemSub1"].Value];
                    t_itemSub2 = t_itemSub1?[m.Groups["ItemSub2"].Value];
                    newValue = t_itemSub2?.ToString() ?? "[INVALID VARIABLE]";
                    break;
                case 25: // FIX ME
                         //Object = 1
                         //Item = 4
                         //ItemSub1 = 8
                         //ItemSub2 = 10
                         //Format = 2
                    t_obj = this.GetType()?.GetProperty(m.Groups["Object"].Value)?.GetValue(this);
                    t_item = t_obj.GetType()?.GetProperty(m.Groups["Item"].Value)?.GetValue(t_obj);
                    t_itemSub1 = t_item?[m.Groups["ItemSub1"].Value];
                    t_itemSub2 = t_itemSub1?[m.Groups["ItemSub2"].Value];
                    try
                    { newValue = t_itemSub2?.ToString(m.Groups["Format"].Value) ?? "[INVALID VARIABLE]"; }
                    catch { newValue = t_itemSub2?.ToString() ?? "[INVALID VARIABLE]"; }
                    break;
                default:
                    break;
            }
            return newValue;
        }

        private int ConfigVariable_PartScore(Match m)
        {
            int PartsValue = 0;
            foreach (var grpName in m.Groups.Cast<Group>()
                                            .Where(g => g.Value != "")
                                            .Select(g => g.Name)
                                            )
            {
                switch (grpName)
                {
                    case "Object":
                        PartsValue += 1;
                        break;
                    case "Item":
                        PartsValue += 4;
                        break;
                    case "ItemSub1":
                        PartsValue += 8;
                        break;
                    case "ItemSub2":
                        PartsValue += 10;
                        break;
                    case "Format":
                        PartsValue += 2;
                        break;
                    default:
                        //Do not add anything
                        break;
                }
            }
            return PartsValue;
        }
    }
}
