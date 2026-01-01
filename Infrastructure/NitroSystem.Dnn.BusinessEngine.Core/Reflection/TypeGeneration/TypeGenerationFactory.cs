using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.BrtPath.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
{
    public sealed class TypeGenerationFactory
    {
        private readonly AssemblyBuilderHost _host;
        private readonly GeneratedModelRegistry _registry;

        private readonly ConcurrentDictionary<string, Type> _cache = new();
        private readonly object _buildLock = new object();

        public TypeGenerationFactory(
            AssemblyBuilderHost host,
            GeneratedModelRegistry registry)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public Type GetOrBuild(ModelDefinition model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Model.Name is required");

            var stableKey = model.ComputeStableKey();

            return _cache.GetOrAdd(stableKey, _ =>
                BuildType(model, stableKey));
        }

        public bool TryGetFromCache(string stableKey, out Type type)
            => _cache.TryGetValue(stableKey, out type);

        public Assembly GetDynamicAssembly() => _host.Assembly;

        public void SaveAssembly() => _host.SaveIfPossible();

        private Type BuildType(ModelDefinition model, string stableKey)
        {
            lock (_buildLock)
            {
                if (_cache.TryGetValue(stableKey, out var existing))
                    return existing;

                var fullName = model.Namespace + "." + model.Name;

                var typeBuilder = _host.Module.DefineType(
                    fullName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.Sealed |
                    TypeAttributes.BeforeFieldInit);

                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                foreach (var prop in model.Properties)
                {
                    AddAutoProperty(typeBuilder, prop);
                }

                var createdType = typeBuilder.CreateType();

                var descriptor = new GeneratedModelDescriptor(
                    model.Name,
                    model.ModelVersion,
                    stableKey,
                    model.Properties.ToList());

                _registry.Register(createdType, descriptor);

                return createdType;
            }
        }

        private static void AddAutoProperty(
            TypeBuilder typeBuilder,
            IPropertyDefinition p)
        {
            var property = p as PropertyDefinition;
            if (property == null)
                throw new ArgumentException(
                    "Invalid property definition");

            var propType = property.ResolveType();

            var fieldName =
                "_" + char.ToLowerInvariant(property.Name[0]) +
                (property.Name.Length > 1
                    ? property.Name.Substring(1)
                    : string.Empty);

            var field = typeBuilder.DefineField(
                fieldName,
                propType,
                FieldAttributes.Private);

            var propBuilder = typeBuilder.DefineProperty(
                property.Name,
                PropertyAttributes.HasDefault,
                propType,
                Type.EmptyTypes);

            // getter
            var getter = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                propType,
                Type.EmptyTypes);

            var getIl = getter.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            // setter
            var setter = typeBuilder.DefineMethod(
                "set_" + property.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                null,
                new[] { propType });

            var setIl = setter.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            propBuilder.SetGetMethod(getter);
            propBuilder.SetSetMethod(setter);
        }
    }
}
