<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="studio.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Studio" %>

<div id="bEngineStudioBoxs" class="container studio-type-boxs">
    <div class="row">
        <div class="col-sm-4">
            <div class="st-box">
                <small class="note-ribbon">Get Started</small>
                <img src="/Portals/0/Images/logo.png" />
                <hr />
                <p class="box-text">Open studio in standalone page(high performance)</p>
                <a href="/DesktopModules/BusinessEngine/studio.aspx?p=<%=this.PortalId%>&a=<%=this.PortalAlias.PortalAliasID%>" target="_blank" class="box-button" >Goto Studio Panel</a>
            </div>
        </div>
    </div>
</div>

<div id="bEngineStudioApp"></div>

<script type="text/javascript">
    window.bEngineGlobalSettings = {
        scenarioName: '<%=this.ScenarioNameParam%>',
        portalId: parseInt('<%=this.PortalId%>'),
        portalAliasId: parseInt('<%=this.PortalAlias.PortalAliasID%>'),
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

