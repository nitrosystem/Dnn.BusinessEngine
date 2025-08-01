<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="module.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Module" EnableViewState="false" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../controls/page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource ID="CtlPageResource" runat="server" />

<div b-ng-app="BusinessEngineClientApp" data-m="<%=this.ModuleGuid%>" class="b-engine-module  <%=this.bRtlCssClass%>"
    ng-controller="moduleController as $"
    ng-init="$.onInitModule(<%=this.ModuleId%>,'<%=this.ModuleGuid%>', '<%=this.ConnectionId%>')">
    <div id="pnlTemplate" runat="server"></div>
</div>

<asp:LinkButton ID="lnkModuleBuilder" CssClass="--b-module-builder" Text="Studio" runat="server"></asp:LinkButton>

<script type="text/javascript">
    var bAppRegistered = [];

    $(document).ready(function () {
        $('*[b-ng-app]').each(function () {
            const module = $(this).data('m');
            if (module && bAppRegistered.indexOf(module) == -1) {
                angular.bootstrap($(this), ['BusinessEngineClientApp']);

                bAppRegistered.push(module);
            }
        });
    });
</script>

