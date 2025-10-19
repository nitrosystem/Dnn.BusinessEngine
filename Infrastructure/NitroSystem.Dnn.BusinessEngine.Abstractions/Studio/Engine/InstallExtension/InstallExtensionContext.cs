using System.Threading;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension
{
    public class InstallExtensionContext : EngineContext, IEngineContext
    {
        public CancellationTokenSource CancellationTokenSource { get; }
        public override CancellationToken CancellationToken => CancellationTokenSource.Token;
        public ExtensionManifest Manifest { get; set; }
        public IUnitOfWork UnitOfWork { get; set; }
        public string UnzipedPath { get; set; }
        public string CurrentVersion { get; set; }

        public InstallExtensionContext(CancellationTokenSource ct)
        {
            CancellationTokenSource = ct;
        }
    }
}
