<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="studio.aspx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.Studio.Web.Studio" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" ng-app="BusinessEngineStudioApp">
<head runat="server">
    <title>Business Engine Studio</title>
</head>
<body>
    <div ng-controller="studioController as $">
        <studio></studio>
    </div>

    <script type="text/javascript">
        var require = { paths: { 'vs': '/DesktopModules/BusinessEngine/client-resources/components/monaco-editor/0.47.0/vs' } };

        window.bEngineGlobalSettings = {
            scenarioName: '<%=ScenarioId!=Guid.Empty?this.ScenarioNameParam:string.Empty%>',
            portalId: parseInt('<%=this.PortalIdParam%>'),
            portalAliasId: parseInt('<%=this.PortalAliasIdParam%>'),
            dnnModuleId: parseInt('<%=this.DnnModuleIdParam%>'),
            moduleId: '<%=this.ModuleIdParam%>',
            moduleType: '<%=this.ModuleTypeParam%>',
            scenarioId: '<%=this.ScenarioId%>',
            siteRoot: '<%=this.SiteRoot%>',
            apiBaseUrl: '<%=this.ApiBaseUrl%>',
            modulePath: '/DesktopModules/BusinessEngine/',
            version: '<%=this.Version%>',
            debugMode: false
        };
    </script>

    <asp:PlaceHolder Id="pnlAntiForgery" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder Id="pnlResources" runat="server"></asp:PlaceHolder>
</body>
</html>
