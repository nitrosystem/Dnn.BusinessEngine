using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NitroSystem.Dnn.BusinessEngine.Test.Studio.Engine
{
    public class ActionServiceTests
    {
        private readonly IActionService _actionService;

        public ActionServiceTests()
        {
            var connection = new SqlConnection("Data Source=.;Initial Catalog=dnndev.new;User ID=sa;Password=12345;MultipleActiveResultSets=True;");
            connection.Open();

            var unitOfWork = new UnitOfWork(connection);
            var cacheBase = new CacheServiceBase();
            var repository = new RepositoryBase(unitOfWork, cacheBase);

            _actionService = new ActionService(unitOfWork, repository);
        }

        [Fact]
        public async Task GetActionTypesViewModel()
        {
            var actionTypes = await _actionService.GetActionTypesViewModelAsync();

            Assert.True(actionTypes.Any());
        }
    }
}
