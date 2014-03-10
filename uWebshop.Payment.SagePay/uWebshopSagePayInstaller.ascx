<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopSagePayInstaller.ascx.cs" Inherits="uWebshop.Payment.SagePayInstaller" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>SagePay payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the SagePay Payment Provider in your store.</p>
	<p>There are some properties to be set on the SagePay node after installation before SagePay will work.</p>
	<p>If you need any help please visit <a href="http://support.uwebshop.com" title="uWebshop Support">our support site</a></p>
                    
		<MerchantId>#YOUR SagePay MerchantId#</MerchantId>      
							<CurrencyCode>978</CurrencyCode>
							<normalReturnUrl>http://www.yoursite.com</normalReturnUrl>

	<asp:Label runat="server" ID="lblSagePayVendorName" AssociatedControlID="txtSagePayVendorName" Text="Your SagePay VendorName: "/>
	<asp:TextBox runat="server" ID="txtSagePayVendorName"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="InstallConfig" Text="Install"/>
</div>

</div></div>