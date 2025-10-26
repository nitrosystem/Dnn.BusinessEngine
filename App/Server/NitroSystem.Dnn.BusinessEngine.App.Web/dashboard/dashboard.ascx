<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="dashboard.ascx.cs" Inherits="NitroSystem.Dnn.BusinessEngine.App.Web.Modules.Dashboard" %>
<%@ Register TagPrefix="b" TagName="PageResource" Src="../controls/page-resources.ascx" %>

<asp:PlaceHolder ID="pnlAntiForgery" runat="server"></asp:PlaceHolder>
<b:PageResource id="CtlPageResource" runat="server" />

<asp:LinkButton ID="lnkDashboardBuilder" CssClass="--b-module-builder" Text="Module Builder" runat="server"></asp:LinkButton>

<div b-ng-app="BusinessEngineClientApp"></div>


<script type="text/javascript">
</script>


