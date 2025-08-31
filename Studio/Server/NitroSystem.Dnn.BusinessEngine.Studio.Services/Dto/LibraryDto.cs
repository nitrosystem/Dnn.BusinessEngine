using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class LibraryDto
    {
        public Guid Id { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public int LoadOrder { get; set; }
        public IEnumerable<LibraryResourceDto> Resources { get; set; }
    }
}
