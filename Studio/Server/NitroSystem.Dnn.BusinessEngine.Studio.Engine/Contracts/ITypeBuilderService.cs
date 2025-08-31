using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface ITypeBuilderService
    {
        string BuildAppModelAsType(AppModelDto appModel, string ouputPath);
    }
}
