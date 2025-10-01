using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration
{
    public sealed class TypeGenerationFactory
    {
        private readonly AssemblyBuilderHost _host;
        private readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();
        private readonly object _buildLock = new object();

        public TypeGenerationFactory(AssemblyBuilderHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public Type GetOrBuild(ModelDefinition model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Name)) throw new ArgumentException("Model.Name is required");


            var key = model.ComputeStableKey();
            return _cache.GetOrAdd(key, _ => BuildType(model, key));
        }

        public bool TryGetFromCache(string stableKey, out Type type) => _cache.TryGetValue(stableKey, out type);

        public void SaveAssembly() => _host.SaveIfPossible();

        public Assembly GetDynamicAssembly() => _host.Assembly;

        private Type BuildType(ModelDefinition model, string stableKey)
        {
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

        private static void AddAutoProperty(TypeBuilder typeBuilder, PropertyDefinition p)
        {
            if (string.IsNullOrWhiteSpace(p.Name))
                throw new ArgumentException("Property name is required");

            var propType = p.ResolveType();

            var fieldName = "_" + char.ToLowerInvariant(p.Name[0]) + (p.Name.Length > 1 ? p.Name.Substring(1) : string.Empty);
            var field = typeBuilder.DefineField(fieldName, propType, FieldAttributes.Private);

            var prop = typeBuilder.DefineProperty(p.Name, PropertyAttributes.HasDefault, propType, Type.EmptyTypes);

            // get_{Name}()
            var getMthdBldr = typeBuilder.DefineMethod(
            "get_" + p.Name,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            propType,
            Type.EmptyTypes);
            var getIl = getMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            // set_{Name}(T value)
            var setMthdBldr = typeBuilder.DefineMethod(
            "set_" + p.Name,
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
