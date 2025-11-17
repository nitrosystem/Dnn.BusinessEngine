<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="dashboard.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Dashboard" %>
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

    const appElements = [];
    const findBAppElementsDeep = (element) => {
        if (element.hasAttribute && element.hasAttribute('b-controller')) {
            appElements.push(element);
        }

        for (const child of element.children) {
            findBAppElementsDeep(child);
        }
    }

    const dashboardElement = document.getElementById('<%=pnlTemplate.ClientID%>');
    findBAppElementsDeep(dashboardElement);

    appElements.forEach(appElement => {
        BusinessEngineApp.bootstrap(appElement);
    });
</script>
