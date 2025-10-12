<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="module.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Module" EnableViewState="false" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../controls/page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource ID="CtlPageResource" runat="server" />

<asp:LinkButton ID="lnkModuleBuilder" CssClass="b--module-builder" Text="Studio" runat="server"></asp:LinkButton>

<div b-app="" id="pnlTemplate" runat="server"></div>

<script type="module">
    import BusinessEngineApp from "/DesktopModules/BusinessEngine/client-app/business-engine.esm.js";

    const appElement = document.querySelector('[b-app]');
    BusinessEngineApp.bootstrap(appElement);
</script>
