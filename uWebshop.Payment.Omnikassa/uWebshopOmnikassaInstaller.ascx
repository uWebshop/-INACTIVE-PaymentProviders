<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopOmnikassaInstaller.ascx.cs" Inherits="uWebshop.Payment.OmnikassaInstaller" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>Ogone payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the Ogone Payment Provider in your store.</p>
	<p>There are some properties to be set on the Ogone node after installation before Ogone will work.</p>
	<p>If you need any help please visit our <a target="_blank" href="http://docs.uWebshop.com">documentation</a> or <a href="http://support.uwebshop.com" title="uWebshop Support">support site</a></p>
     
	<asp:Label runat="server" ID="lblMerchantId" AssociatedControlID="txtMerchantId" Text="Your Omnikassa MerchantId: "/>
	<asp:TextBox runat="server" ID="txtMerchantId"/>
	<asp:Label runat="server" ID="lblSecurityKey" AssociatedControlID="txtSecurityKey" Text="Your Omnikassa Security Key: "/>
	<asp:TextBox runat="server" ID="txtSecurityKey"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="InstallConfig" Text="Install"/>
</div>

</div></div>