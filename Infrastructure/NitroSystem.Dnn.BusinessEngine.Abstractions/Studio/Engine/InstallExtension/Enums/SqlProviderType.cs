using System.ComponentModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Enums
{
    public enum SqlProviderType
    {
        [Description("Install")]
        Install = 1,
        [Description("Uninstall")]
        Uninstall = 2
    }
}
