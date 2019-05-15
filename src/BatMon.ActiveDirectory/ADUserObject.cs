/* 
 *Copyright (C) 2019 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using BatMon.Framework;
using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace BatMon.ActiveDirectory
{
    public class ADUserObject : BatMonPlugin, IBatMonPlugin
    {
        protected override Result[] fetchResults()
        {
            return null;
        }
    }
}
