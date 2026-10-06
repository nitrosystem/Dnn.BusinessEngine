using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using Xunit;
using FluentAssertions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Template;
using NitroSystem.Dnn.BusinessEngine.Repository.Template;

namespace NitroSystem.Dnn.BusinessEngine.Tests.Studio.ApplicationService.Template
{
    [Collection(TemplateMappingCollection.Name)]
    public class TemplateServiceTests
    {
        // Helper to build the service with a controllable IRepository mock.
        // IModuleService is unused by GetTemplatesViewModelAsync today (per
        // current TemplateService code), so a bare mock is enough — if that
        // changes later, set up the specific calls this test needs here.
        private static (TemplateService service, Mock<ISql> repoMock) CreateSut(
            List<TemplateInfo> templates,
            List<TemplateThemeInfo> themes)
        {
            var repoMock = new Mock<ISql>();

            repoMock
                .Setup(r => r.GetAllAsync<TemplateInfo>("ViewOrder"))
                .ReturnsAsync(templates);

            repoMock
                .Setup(r => r.GetAllAsync<TemplateThemeInfo>("ViewOrder"))
                .ReturnsAsync(themes);

            var moduleServiceMock1 = new Mock<TemplateRepository>();
            var moduleServiceMock2 = new Mock<TemplateThemeRepository>();

            var service = new TemplateService(moduleServiceMock1.Object, moduleServiceMock2.Object);
            return (service, repoMock);
        }

        private static TemplateInfo BuildTemplate(
            Guid id,
            Guid? parentId,
            int moduleType,
            string templateName,
            int viewOrder = 1)
        {
            return new TemplateInfo
            {
                Id = id,
                ExtensionId = Guid.NewGuid(),
                ParentId = parentId,
                ModuleType = moduleType,
                TemplateName = templateName,
                TemplateImage = $"{templateName}.png",
                TemplatePath = $"/templates/{templateName}/template.html",
                TemplateCssPath = $"/templates/{templateName}/style.css",
                PreviewImages = $"/templates/{templateName}/preview1.png;/templates/{templateName}/preview.png",
                Description = $"Description for {templateName}"
            };
        }

        private static TemplateThemeInfo BuildTheme(Guid id, Guid templateId, string themeName, int viewOrder = 1)
        {
            return new TemplateThemeInfo
            {
                Id = id,
                TemplateId = templateId,
                ThemeName = themeName,
                ThemeImage = $"{themeName}.png",
                ThemeCssPath = $"/themes/{themeName}/style.css",
                ThemeCssClass = themeName.ToLowerInvariant().Replace(" ", "-"),
                ViewOrder = viewOrder
            };
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_Returns_Only_Templates_Matching_ParentId_And_ModuleType()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var otherParentId = Guid.NewGuid();

            var templates = new List<TemplateInfo>
            {
                BuildTemplate(Guid.NewGuid(), parentId, 0, "Generic - matches any module type", 1),
                BuildTemplate(Guid.NewGuid(), parentId, (int)ModuleType.Form, "Form specific - matches", 2),
                BuildTemplate(Guid.NewGuid(), parentId, (int)ModuleType.List, "List specific - should NOT match", 3),
                BuildTemplate(Guid.NewGuid(), otherParentId, 0, "Wrong parent - should NOT match", 4),
            };

            var (service, _) = CreateSut(templates, new List<TemplateThemeInfo>());

            // Act
            var result = await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, parentId);

            // Assert
            result.Should().HaveCount(2);
            result.Select(r => r.TemplateName).Should().BeEquivalentTo(
                "Generic - matches any module type",
                "Form specific - matches");
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_Attaches_Matching_Themes_To_Each_Template()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var otherTemplateId = Guid.NewGuid();

            var templates = new List<TemplateInfo>
            {
                BuildTemplate(templateId, parentId, 0, "Default", 1),
            };

            var themes = new List<TemplateThemeInfo>
            {
                BuildTheme(Guid.NewGuid(), templateId, "Theme A", 1),
                BuildTheme(Guid.NewGuid(), templateId, "Theme B", 2),
                BuildTheme(Guid.NewGuid(), otherTemplateId, "Theme for a different template", 1),
            };

            var (service, _) = CreateSut(templates, themes);

            // Act
            var result = (await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, parentId)).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Themes.Should().HaveCount(2);
            result[0].Themes.Select(t => t.ThemeName).Should().BeEquivalentTo("Theme A", "Theme B");
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_Maps_Scalar_Fields_Correctly()
        {
            // Arrange — verifies HybridMapper's name-based property mapping
            // picks up every scalar field correctly for a single template
            // with no themes attached.
            var parentId = Guid.NewGuid();
            var templateId = Guid.NewGuid();

            var template = BuildTemplate(templateId, parentId, 0, "Default Template", viewOrder: 7);

            var (service, _) = CreateSut(new List<TemplateInfo> { template }, new List<TemplateThemeInfo>());

            // Act
            var result = (await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, parentId)).Single();

            // Assert
            result.Id.Should().Be(templateId);
            result.ParentId.Should().Be(parentId);
            result.TemplateName.Should().Be("Default Template");
            result.TemplateImage.Should().Be(template.TemplateImage);
            result.TemplatePath.Should().Be(template.TemplatePath);
            result.TemplateCssPath.Should().Be(template.TemplateCssPath);
            result.Description.Should().Be(template.Description);
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_PreviewImages_Is_Deserialized_From_Json()
        {
            // TemplateInfo.PreviewImages is stored as a JSON-encoded string
            // (e.g. ["/a.png","/b.png"]). HybridMapper's plain name-matching
            // would skip this field (string is not assignable to
            // IEnumerable<string>), but TemplateMappingProfile registers an
            // AfterMap hook that explicitly deserializes it via
            // ReflectionUtil.TryJsonCasting<IEnumerable<string>>. That hook
            // only runs because [Collection(TemplateMappingCollection.Name)]
            // guarantees TemplateMappingProfile.Register() has already run
            // for this test collection.
            var parentId = Guid.NewGuid();
            var template = BuildTemplate(Guid.NewGuid(), parentId, 0, "Has preview images");
            template.PreviewImages = "[\"/templates/has-preview-images/preview1.png\",\"/templates/has-preview-images/preview2.png\"]";

            var (service, _) = CreateSut(new List<TemplateInfo> { template }, new List<TemplateThemeInfo>());

            // Act
            var result = (await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, parentId)).Single();

            // Assert
            result.PreviewImages.Should().BeEquivalentTo(new[]
            {
                "/templates/has-preview-images/preview1.png",
                "/templates/has-preview-images/preview2.png"
            });
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_Returns_Empty_When_No_Template_Matches_ParentId()
        {
            // Arrange
            var templates = new List<TemplateInfo>
            {
                BuildTemplate(Guid.NewGuid(), Guid.NewGuid(), 0, "Belongs elsewhere"),
            };

            var (service, _) = CreateSut(templates, new List<TemplateThemeInfo>());

            // Act
            var result = await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTemplatesViewModelAsync_Queries_Repository_With_ViewOrder_Sort()
        {
            // Arrange — verifies the repository contract is called the way we expect,
            // independent of the actual filtering/mapping result.
            var (service, repoMock) = CreateSut(new List<TemplateInfo>(), new List<TemplateThemeInfo>());

            // Act
            await service.GetTemplatesViewModelAsync(ModuleWrapper.DnnPage, ModuleType.Form, null);

            // Assert
            repoMock.Verify(r => r.GetAllAsync<TemplateInfo>("ViewOrder"), Times.Once);
            repoMock.Verify(r => r.GetAllAsync<TemplateThemeInfo>("ViewOrder"), Times.Once);
        }
    }
}
