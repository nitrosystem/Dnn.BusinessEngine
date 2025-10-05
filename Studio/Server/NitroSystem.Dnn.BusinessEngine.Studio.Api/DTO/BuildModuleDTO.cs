using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class BuildModuleDto
    {
        public Guid ModuleId { get; set; }
        public Guid? ParentID { get; set; }
        public int PageId { get; set; }
    }
}
