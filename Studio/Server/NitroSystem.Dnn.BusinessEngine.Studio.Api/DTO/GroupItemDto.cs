using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto
{
    public class GroupItemDto
    {
        public Guid? GroupId { get; set; }
        public Guid ItemId { get; set; }
        public string GroupType { get; set; }
    }
}
