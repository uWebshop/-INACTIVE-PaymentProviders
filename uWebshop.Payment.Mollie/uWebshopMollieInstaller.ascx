<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopMollieInstaller.ascx.cs" Inherits="uWebshop.Payment.MollieInstaller" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>Mollie payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the Mollie Payment Provider in your store.</p>
	<p>There are some properties to be set on the Mollie node after installation before Mollie will work.</p>
	<p>If you need any help please visit our <a target="_blank" href="http://docs.uWebshop.com">documentation</a> or <a href="http://support.uwebshop.com" title="uWebshop Support">support site</a></p>
                    
	<asp:Label runat="server" ID="lblMolliePartnerId" AssociatedControlID="txtMolliePartnerId" Text="Your Mollie PartnerId: "/>
	<asp:TextBox runat="server" ID="txtMolliePartnerId"/>
	<asp:Button runat="server" ID="btnInstallMollie" OnClick="InstallConfig" Text="Install"/>
</div>

</div></div>