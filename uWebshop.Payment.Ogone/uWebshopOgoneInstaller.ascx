<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopOgoneInstaller.ascx.cs" Inherits="uWebshop.Payment.OgoneInstaller" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>Ogone payment provider for uWebshop</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the Ogone Payment Provider in your store.</p>
	<p>There are some properties to be set on the Ogone node after installation before Ogone will work.</p>
	<p>If you need any help please visit our <a target="_blank" href="http://docs.uWebshop.com">documentation</a> or <a href="http://support.uwebshop.com" title="uWebshop Support">support site</a></p>
                    
	<asp:Label runat="server" ID="lblOgonePSPID" AssociatedControlID="txtOgonePSPID" Text="Your Ogone PSPID: "/>
	<asp:TextBox runat="server" ID="txtOgonePSPID"/>
	<asp:Label runat="server" ID="lblSHA" AssociatedControlID="txtSHA" Text="Your Ogone SHA Signature: "/>
	<asp:TextBox runat="server" ID="txtSHA"/>
	<asp:Button runat="server" ID="btnInstallOgone" OnClick="InstallOgoneConfig" Text="Install"/>
</div>