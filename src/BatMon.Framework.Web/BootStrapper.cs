 /* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using Nancy;
using Nancy.Conventions;

namespace BatMon.Framework.Web
{
    public class BootStrapper: DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Clear();

            nancyConventions.StaticContentsConventions.Add
            (StaticContentConventionBuilder.AddDirectory("css", "/web/style"));

            nancyConventions.StaticContentsConventions.Add
            (StaticContentConventionBuilder.AddDirectory("js", "/web/js"));

            nancyConventions.StaticContentsConventions.Add
            (StaticContentConventionBuilder.AddDirectory("images", "/web/img"));

            nancyConventions.StaticContentsConventions.Add
            (StaticContentConventionBuilder.AddDirectory("fonts", "/web/fonts"));

            nancyConventions.StaticContentsConventions.Add
            (StaticContentConventionBuilder.AddDirectory("Static", @"Static"));

        }
    }
}
