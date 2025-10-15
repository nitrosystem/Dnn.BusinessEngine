using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration
{
    public sealed class TypeGenerationFactory
    {
        private readonly IBrtGateService _brtGate;
        private readonly AssemblyBuilderHost _host;
        private readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();
        private readonly object _buildLock = new object();

        public TypeGenerationFactory(AssemblyBuilderHost host, IBrtGateService brtGate)
        {
            _brtGate = brtGate ?? throw new ArgumentNullException(nameof(brtGate));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public Type GetOrBuild(ModelDefinition model, Guid? permitId = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Model.Name is required");

            var key = model.ComputeStableKey();
            return _cache.GetOrAdd(key, _ => BuildType(model, key, permitId.Value));
        }

        public bool TryGetFromCache(string stableKey, out Type type) => _cache.TryGetValue(stableKey, out type);

        public void SaveAssembly() => _host.SaveIfPossible();

        public Assembly GetDynamicAssembly() => _host.Assembly;

        private Type BuildType(ModelDefinition model, string stableKey, Guid permitId)
        {
            if (!AsyncHelper.RunSync<bool>(() => _brtGate.IsGateOpenAsync(permitId)))
            {
                throw new UnauthorizedAccessException("Operation must run inside BRT gate.");
            }

            lock (_buildLock)
            {
                if (_cache.TryGetValue(stableKey, out var existing)) return existing;

                var typeBuilder = _host.Module.DefineType(
                model.Namespace + "." + model.Name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

                // Public parameterless ctor
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                // [GeneratedModel(Name, Version, StableKey)]
                var attrCtor = typeof(GeneratedModelAttribute).GetConstructor(new[] { typeof(string), typeof(string), typeof(string) });
                var attrBuilder = new CustomAttributeBuilder(attrCtor, new object[] { model.Name, model.ModelVersion, stableKey });
                typeBuilder.SetCustomAttribute(attrBuilder);

                foreach (var p in model.Properties)
                {
                    AddAutoProperty(typeBuilder, p);
                }

                var created = typeBuilder.CreateType();
                return created;
            }
        }

        private static void AddAutoProperty(TypeBuilder typeBuilder, IPropertyDefinition p)
        {
            var propertyDef = p as PropertyDefinition;

            if (string.IsNullOrWhiteSpace(propertyDef.Name))
                throw new ArgumentException("Property name is required");

            var propType = propertyDef.ResolveType();

            var fieldName = "_" + char.ToLowerInvariant(propertyDef.Name[0]) + (propertyDef.Name.Length > 1 ? propertyDef.Name.Substring(1) : string.Empty);
            var field = typeBuilder.DefineField(fieldName, propType, FieldAttributes.Private);

            var prop = typeBuilder.DefineProperty(propertyDef.Name, PropertyAttributes.HasDefault, propType, Type.EmptyTypes);

            // get_{Name}()
            var getMthdBldr = typeBuilder.DefineMethod(
            "get_" + propertyDef.Name,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            propType,
            Type.EmptyTypes);
            var getIl = getMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            // set_{Name}(T value)
            var setMthdBldr = typeBuilder.DefineMethod(
            "set_" + propertyDef.Name,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null,
            new[] { propType });
            var setIl = setMthdBldr.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            prop.SetGetMethod(getMthdBldr);
            prop.SetSetMethod(setMthdBldr);
        }
    }
}
