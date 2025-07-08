using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public static class ServiceLocator<T> where T : class
    {
        private static T _instance;

        public static void Init(string typeName, params object[] constructor)
        {
            try
            {
                _instance = Activator.CreateInstance(Type.GetType(typeName), constructor) as T;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static T CreateInstance(string typeName, params object[] constructor)
        {
            if (_instance == null || typeName.IndexOf(_instance.GetType().FullName) == -1) Init(typeName, constructor);

            return _instance;
        }
    }
}
