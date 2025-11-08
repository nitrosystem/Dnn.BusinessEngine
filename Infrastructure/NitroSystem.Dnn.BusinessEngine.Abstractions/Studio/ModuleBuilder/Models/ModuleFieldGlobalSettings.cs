namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models
{
    public class ModuleFieldGlobalSettings
    {
        public bool IsContentField { get; set; }
        public string Placeholder { get; set; }
        public string Subtext { get; set; }
        public bool EnableValidationPattern { get; set; }
        public string ValidationPattern { get; set; }
        public string ValidationMessage { get; set; }
        public string CssClass { get; set; }
        public string FieldTextCssClass { get; set; }
        public string SubtextCssClass { get; set; }
        public string RequiredMessageCssClass { get; set; }
        public bool AllowMultiple { get; set; }
        public bool ShowLabel { get; set; }
        public bool WithIcon { get; set; }
        public string IconPosition { get; set; }
    }
}
