using System;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.Database
{
    public class CustomQueryResult
    {
        public Guid ItemID { get; set; }
        public Guid ActionID { get; set; }
        public string LeftExpression { get; set; }
        public string EvalType { get; set; }
        public string RightExpression { get; set; }
    }
}
