<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="studio.aspx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Studio" %>

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
            scenarioId: '<%=this.ScenarioId%>',
            scenarioName: '<%=this.ScenarioName%>',
            siteRoot: '<%=this.SiteRoot%>',
        };
    </script>

    <asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="pnlResources" runat="server"></asp:PlaceHolder>
</body>
</html>
