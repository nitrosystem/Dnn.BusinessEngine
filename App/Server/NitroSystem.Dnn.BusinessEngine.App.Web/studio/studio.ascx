<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="studio.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.Modules.Studio" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div id="bEngineStudioBoxs" class="container studio-type-boxs">
    <div class="row">
        <div class="col-sm-4">
            <div class="st-box">
                <small class="note-ribbon">Get Started</small>
                <img src="/Portals/0/Images/logo.png" />
                <hr />
                <p class="box-text">Open studio in standalone page(high performance)</p>
                <a href="/DesktopModules/BusinessEngine/studio.aspx?p=<%=this.PortalId%>&a=<%=this.PortalAlias.PortalAliasId%>" target="_blank" class="box-button" >Goto Studio Panel</a>
            </div>
        </div>
    </div>
</div>

<div id="bEngineStudioApp"></div>

<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/BusinessEngine/Scripts/studio-setup.js"></dnn:DnnJsInclude>

<script type="text/javascript">
    window.bEngineGlobalSettings = {
        scenarioName: '<%=this.ScenarioNameParam%>',
        portalId: parseInt('<%=this.PortalId%>'),
        portalAliasId: parseInt('<%=this.PortalAlias.PortalAliasId%>'),
        userId: parseInt('<%=this.UserId%>'),
        dnnModuleId: parseInt('<%=this.ModuleId%>'),
        dnnTabId: parseInt('<%=this.DnnTabId%>'),
        moduleId: '<%=this.ModuleGuid%>',
        moduleType: '<%=this.ModuleTypeParam%>',
        scenarioId: '<%=this.ScenarioId%>',
        siteRoot: '<%=this.SiteRoot%>',
        apiBaseUrl: '<%=this.ApiBaseUrl%>',
        modulePath: '/DesktopModules/BusinessEngine/',
        version: '<%=this.Version%>',
        debugMode: false
    };
</script>

