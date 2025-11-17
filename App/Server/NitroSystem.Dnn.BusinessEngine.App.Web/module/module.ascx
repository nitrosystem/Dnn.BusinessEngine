<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="module.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Module" EnableViewState="false" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../controls/page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource ID="CtlPageResource" runat="server" />

<%if (IsAdmin)
    {%>
<a href="<%=StudioUrl%>" target="_blank" class="b--module-builder">Studio</a>
<%} %>

<div id="pnlTemplate" runat="server"></div>

<script type="module">
    import BusinessEngineApp from "/DesktopModules/BusinessEngine/client-app/business-engine.esm.js?ver=<%=this.Version%>";

    const appElement = document.getElementById('<%=pnlTemplate.ClientID%>');
    BusinessEngineApp.bootstrap(appElement);
</script>
