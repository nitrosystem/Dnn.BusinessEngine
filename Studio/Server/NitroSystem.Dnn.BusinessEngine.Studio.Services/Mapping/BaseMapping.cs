using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class BaseMapping<TEntity, TViewModel>
                                where TEntity : class, IEntity, new()
                                where TViewModel : class, IViewModel, new()
    {
        public static IEnumerable<TViewModel> MapViewModels(IEnumerable<TEntity> entities)
        {
            var mapper = new ExpressionMapper<TEntity, TViewModel>();
                return entities.Select(entity => mapper.Map(entity));
        }

        public static TViewModel MapViewModel(TEntity entity)
        {
            var mapper = new ExpressionMapper<TEntity, TViewModel>();
            return mapper.Map(entity);
        }

        public static IEnumerable<TEntity> MapEntities(IEnumerable<TViewModel> entities)
        {
            var mapper = new ExpressionMapper<TViewModel, TEntity>();
            return entities.Select(entity => mapper.Map(entity));
        }

        public static TEntity MapEntity(TViewModel viewModel)
        {
            var mapper = new ExpressionMapper<TViewModel, TEntity>();
            return mapper.Map(viewModel);
        }
    }
}
