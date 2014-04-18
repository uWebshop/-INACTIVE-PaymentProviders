<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopPayPalInstaller.ascx.cs" Inherits="uWebshop.Payment.PayPalInstaller" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>PayPal payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the PayPal Payment Provider in your store.</p>
	<p>There are some properties to be set on the PayPal node after installation before PayPal will work.</p>
	<p>If you need any help please visit our <a target="_blank" href="http://docs.uWebshop.com">documentation</a> or <a href="http://support.uwebshop.com" title="uWebshop Support">support site</a></p>
                    
	<asp:Label runat="server" ID="lblPayPalAccountId" AssociatedControlID="txtPayPalAccountId" Text="Your Paypal Account ID/Email: "/>
	<asp:TextBox runat="server" ID="txtPayPalAccountId"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="installConfig" Text="Install"/>
</div>

</div></div>