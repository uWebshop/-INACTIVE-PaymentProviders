<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopSisowInstaller.ascx.cs" Inherits="uWebshop.Payment.SisowInstaller" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>Sisow payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the Sisow Payment Provider in your store.</p>
	<p>There are some properties to be set on the Sisow node after installation before Sisow will work.</p>
	<p>If you need any help please visit <a href="http://support.uwebshop.com" title="uWebshop Support">our support site</a></p>
                    
	<asp:Label runat="server" ID="lblId" AssociatedControlID="txtId" Text="Your Sisow merchant Id : "/>
	<asp:TextBox runat="server" ID="txtId"/>
	<asp:Label runat="server" ID="lblKey" AssociatedControlID="txtKey" Text="Your Sisow merchant key "/>
	<asp:TextBox runat="server" ID="txtKey"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="InstallConfig" Text="Install"/>
</div>

</div></div>