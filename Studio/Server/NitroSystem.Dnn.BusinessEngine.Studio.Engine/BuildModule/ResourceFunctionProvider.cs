using NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public static class ResourceFunctionProvider
    {
        public static async Task<string> GetFieldTypeBySkin(
            string fieldType,
            string template,
            (DashboardType DashboardType, string Skin, string SkinPath) dashboard,
            HttpContext context)
        {
            var skin = DashboardSkinManager.GetDashboardSkin(
                dashboard.DashboardType,
                dashboard.Skin,
                dashboard.SkinPath,
                context
            );

            var templateInfo = skin.FieldTypes.FirstOrDefault(f => f.FieldType == fieldType)?
                .Templates.FirstOrDefault(t => t.TemplateName == template);

            return templateInfo.TemplatePath;
        }
    }
}
