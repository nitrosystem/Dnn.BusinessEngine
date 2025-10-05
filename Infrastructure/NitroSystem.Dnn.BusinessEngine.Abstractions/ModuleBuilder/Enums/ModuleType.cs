using System.ComponentModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums
{
    public enum ModuleType
    {
        [Description("Dashboard")]
        Dashboard = 0,

        [Description("Form")]
        Form = 1,

        [Description("List")]
        List = 2,

        [Description("Details")]
        Details = 3
    }
}
