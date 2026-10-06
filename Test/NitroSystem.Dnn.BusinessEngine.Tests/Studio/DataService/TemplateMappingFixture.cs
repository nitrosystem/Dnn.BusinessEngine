using Xunit;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Template;

namespace NitroSystem.Dnn.BusinessEngine.Tests.Studio.ApplicationService.Template
{
    /// <summary>
    /// HybridMapper holds its AfterMap/BeforeMap hooks in static
    /// dictionaries shared by the whole test process. Registering a
    /// mapping profile is therefore a GLOBAL, ONE-TIME side effect —
    /// not something that should happen inside individual [Fact] methods.
    ///
    /// This fixture registers TemplateMappingProfile exactly once, no
    /// matter how many test classes share it or in what order xUnit
    /// decides to run them. xUnit instantiates a class fixture once per
    /// test collection and disposes it after the last test in that
    /// collection finishes, which is exactly the lifetime we want here.
    /// </summary>
    public class TemplateMappingFixture
    {
        public TemplateMappingFixture()
        {
            TemplateMappingProfile.Register();
        }
    }

    /// <summary>
    /// Marker collection. Any test class decorated with
    /// [Collection(TemplateMappingCollection.Name)] shares the SAME
    /// TemplateMappingFixture instance and is guaranteed to run with
    /// the mapping profile already registered — and, just as
    /// importantly, xUnit will not run this collection's tests in
    /// parallel with another collection that also registers
    /// AfterMap hooks for the same (TSource, TDestination) pair,
    /// since collections themselves are the parallelization boundary.
    /// </summary>
    [CollectionDefinition(Name)]
    public class TemplateMappingCollection : ICollectionFixture<TemplateMappingFixture>
    {
        public const string Name = "TemplateMapping";
    }
}
