using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
{
    public sealed class AssemblyBuilderHost
    {
        private readonly string _assemblyName;
        private readonly string _filePath;
        private readonly string _fileName;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private bool _saved;

        public ModuleBuilder Module => _moduleBuilder;
        public AssemblyBuilder Assembly => _assemblyBuilder;

        public AssemblyBuilderHost(string assemblyName, string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyName)) throw new ArgumentNullException(nameof(assemblyName));
            _assemblyName = assemblyName;

            if (!string.IsNullOrEmpty(fileName))
            {
                _filePath = Path.GetDirectoryName(fileName);
                _fileName = Path.GetFileName(fileName);
            }

            var an = new AssemblyName(_assemblyName);
            var access = string.IsNullOrWhiteSpace(_fileName) ? AssemblyBuilderAccess.Run : AssemblyBuilderAccess.RunAndSave;

            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, access, _filePath);

            _moduleBuilder = string.IsNullOrWhiteSpace(_fileName)
            ? _assemblyBuilder.DefineDynamicModule(an.Name)
            : _assemblyBuilder.DefineDynamicModule(an.Name, _fileName);
        }

        public void SaveIfPossible()
        {
            if (_saved) return;

            if (!string.IsNullOrWhiteSpace(_fileName))
            {
                _assemblyBuilder.Save(_fileName);
                _saved = true;
            }
        }
    }
}
