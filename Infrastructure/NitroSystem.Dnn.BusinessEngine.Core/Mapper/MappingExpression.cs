using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    public class MappingExpression<TSource, TDestination>
    {
        private readonly List<Action<object, object>> _actions;

        public MappingExpression(List<Action<object, object>> actions)
        {
            _actions = actions;
        }

        public MappingExpression<TSource, TDestination> ForMember(
            Action<TSource, TDestination> mappingAction)
        {
            _actions.Add((src, dest) => mappingAction((TSource)src, (TDestination)dest));
            return this;
        }
    }


}
