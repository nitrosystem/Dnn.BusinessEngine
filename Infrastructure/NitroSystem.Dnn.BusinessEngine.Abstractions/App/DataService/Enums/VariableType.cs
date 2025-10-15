namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Enums
{
    public enum VariableType
    {
        String,
        Int,
        Long,
        Decimal,
        Float,
        Double,
        Boolean,
        DateTime,
        Guid,

        AppModel,
        Object,

        AppModelList,

        Unknown               // Optional for uninitialized states
    }
}
