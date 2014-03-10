<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopMultiSafePayInstaller.ascx.cs" Inherits="uWebshop.Payment.MultiSafePayInstaller" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>MultiSafepay payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the MultiSafepay Payment Provider in your store.</p>
	<p>There are some properties to be set on the MultiSafepay node after installation before MultiSafepay will work.</p>
	<p>If you need any help please visit <a href="http://support.uwebshop.com" title="uWebshop Support">our support site</a></p>
                    
	<asp:Label runat="server" ID="lblAccountId" AssociatedControlID="txtAccountId" Text="Your MultiSafePay account Id : "/>
	<asp:TextBox runat="server" ID="txtAccountId"/>
	<asp:Label runat="server" ID="lblSiteId" AssociatedControlID="txtSiteId" Text="Your MultiSafePay Site Id: "/>
	<asp:TextBox runat="server" ID="txtSiteId"/>
	<asp:Label runat="server" ID="lblSecureSiteId" AssociatedControlID="txtSecureSiteId" Text="Your MultiSafePay Secure Site Id: "/>
	<asp:TextBox runat="server" ID="txtSecureSiteId"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="InstallConfig" Text="Install"/>
</div>

</div></div>