<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="dashboard.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Dashboard" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource id="CtlPageResource" runat="server" />

<%if (this.DashboardType==2){ %>
<div b-ng-app="BusinessEngineClientApp" data-module="<%=this.ModuleGuid%>" id="pnlDashboard<%=this.ModuleGuid%>" ng-controller="dashboardController as $" ng-init="$.onInitModule('<%=this.ModuleGuid%>','<%=this.ModuleName%>','<%=this.ConnectionID%>')" class="b-engine-module <%=this.bRtlCssClass%>">
    <div id="pnlBusinessEngine<%=this.ModuleGuid%>" data-module="<%=this.ModuleGuid%>" ng-controller="moduleController as $"
        ng-init="$.onInitModule(<%=this.ModuleId%>,'<%=this.ModuleGuid%>','<%=this.ModuleName%>','<%=this.ConnectionID%>')">
    </div>
    <div id="dashboardNgScripts"></div>
</div>
<%} %>

<asp:LinkButton ID="lnkDashboardBuilder" CssClass="--b-module-builder" Text="Module Builder" runat="server"></asp:LinkButton>

<script type="text/javascript">
    var bAppRegistered = [];

    $(document).ready(function () {
        $('*[b-ng-app]').each(function () {
            const module = $(this).data('module');
            if (module && bAppRegistered.indexOf(module) == -1) {
                angular.bootstrap($(this), ['BusinessEngineClientApp']);

                bAppRegistered.push(module);
            }
        });
    });
</script>


