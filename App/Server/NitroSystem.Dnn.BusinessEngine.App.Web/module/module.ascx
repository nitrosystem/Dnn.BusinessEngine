<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="module.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Module" EnableViewState="false" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../controls/page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource ID="CtlPageResource" runat="server" />

<div id="pnlBusinessEngine<%=this.ModuleGuid%>" data-module="<%=this.ModuleGuid%>" class="b-engine-module  <%=this.bRtlCssClass%>"
    b-ng-app="BusinessEngineClientApp"
    ng-controller="moduleController as $"
    ng-init="$.onInitModule(<%=this.ModuleId%>,'<%=this.ModuleGuid%>', '<%=this.ModuleName%>','<%=this.ConnectionID%>')">
    <div id="pnlTemplate" runat="server"></div>
</div>

<asp:LinkButton ID="lnkModuleBuilder" CssClass="--b-module-builder" Text="Studio" runat="server"></asp:LinkButton>

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

