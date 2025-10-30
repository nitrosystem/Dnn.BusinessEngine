using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto
{
    public class DashboardDto
    {
        public Guid Id { get; set; }
        public IEnumerable<string> AuthorizationViewDashboard { get; set; }
        public IEnumerable<DashboardPageDto> Pages { get; set; }

    }
}
