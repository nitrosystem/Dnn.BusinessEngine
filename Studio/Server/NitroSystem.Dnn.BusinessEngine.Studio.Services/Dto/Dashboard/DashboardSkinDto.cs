using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class DashboardSkinDto : IDto
    {
        public DtoType Type => DtoType.ClientOnly;

        public Guid Id { get; set; }
        public string SkinName { get; set; }
        public string Title { get; set; }
        public string SkinImage { get; set; }
        public string SkinPath { get; set; }
    }
}
