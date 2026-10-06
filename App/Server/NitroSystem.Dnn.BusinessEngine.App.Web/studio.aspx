<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="studio.aspx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Studio" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" ng-app="BusinessEngineStudioApp">
<head runat="server">
    <title>Business Engine Studio</title>
    <link rel="shortcut icon" href="/DesktopModules/BusinessEngine/assets/images/fav.ico" type="image/x-icon" />
</head>
<body>
    <div ng-controller="studioController as $">
        <studio></studio>
    </div>

    <script type="text/javascript">
        window.bEngineBaseOptions = {
            scenarioId: '<%=this.ScenarioId%>',
            scenarioName: '<%=this.ScenarioName%>',
            siteRoot: '<%=this.SiteRoot%>',
            version: '<%=this.Version%>',
        };
    </script>

    <asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="pnlResources" runat="server"></asp:PlaceHolder>
</body>
</html>
