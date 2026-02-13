@echo off
setlocal

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Infrastructure\NitroSystem.Dnn.BusinessEngine.Abstractions\obj\Debug\NitroSystem.Dnn.BusinessEngine.Abstractions.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Abstractions.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Infrastructure\NitroSystem.Dnn.BusinessEngine.Shared\obj\Debug\NitroSystem.Dnn.BusinessEngine.Shared.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Shared.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Infrastructure\NitroSystem.Dnn.BusinessEngine.Core\obj\Debug\NitroSystem.Dnn.BusinessEngine.Core.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Core.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Infrastructure\NitroSystem.Dnn.BusinessEngine.Data\obj\Debug\NitroSystem.Dnn.BusinessEngine.Data.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Data.dll"
copy /Y %source% %target%

----------------------------------------------------------------------------------------------------------

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Studio\Server\NitroSystem.Dnn.BusinessEngine.Studio.ImportExport\obj\Debug\NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Studio\Server\NitroSystem.Dnn.BusinessEngine.Studio.Engine\obj\Debug\NitroSystem.Dnn.BusinessEngine.Studio.Engine.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Studio.Engine.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Studio\Server\NitroSystem.Dnn.BusinessEngine.Studio.DataService\obj\Debug\NitroSystem.Dnn.BusinessEngine.Studio.DataService.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Studio.DataService.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\Studio\Server\NitroSystem.Dnn.BusinessEngine.Studio.Api\obj\Debug\NitroSystem.Dnn.BusinessEngine.Studio.Api.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Studio.Api.dll"
copy /Y %source% %target%

----------------------------------------------------------------------------------------------------------

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\App\Server\NitroSystem.Dnn.BusinessEngine.App.Engine\obj\Debug\NitroSystem.Dnn.BusinessEngine.App.Engine.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.App.Engine.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\App\Server\NitroSystem.Dnn.BusinessEngine.App.DataService\obj\Debug\NitroSystem.Dnn.BusinessEngine.App.DataService.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.App.DataService.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\App\Server\NitroSystem.Dnn.BusinessEngine.App.Api\obj\Debug\NitroSystem.Dnn.BusinessEngine.App.Api.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.App.Api.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Version-2\Source\App\Server\NitroSystem.Dnn.BusinessEngine.App.Web\obj\Debug\NitroSystem.Dnn.BusinessEngine.App.Web.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.App.Web.dll"
copy /Y %source% %target%


----------------------------------------------------------------------------------------------------------

set source="D:\BusinessEngine\Dnn.BusinessEngine\Extensions\BasicExtensions\Server\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions\obj\Debug\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.dll"
copy /Y %source% %target%

set source="D:\BusinessEngine\Dnn.BusinessEngine\Extensions\BasicExtensions\Server\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services\obj\Debug\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.dll"
set target="D:\_business-engine-test\test2-9.4.0\wwwroot\bin\NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.dll"
copy /Y %source% %target%


endlocal