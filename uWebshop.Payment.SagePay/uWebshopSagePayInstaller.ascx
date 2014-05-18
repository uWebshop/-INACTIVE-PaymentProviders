<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uWebshopSagePayInstaller.ascx.cs" Inherits="uWebshop.Payment.SagePayInstaller" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>
<link href="/umbraco_client/propertypane/style.css" rel="stylesheet" />

<div class="dashboardWrapper">
	<h2>SagePay Direct payment provider for uWebshop2</h2>
	<img src="/App_Plugins/uWebshop/images/uwebshop32x32.png" alt="uWebshop" class="dashboardIcon" />
	<p>Use the installer below to install the SagePay Payment Provider in your store.</p>
	<p>There are some properties to be set on the SagePay node after installation before SagePay will work.</p>
	<p>If you need any help please visit <a href="http://support.uwebshop.com" title="uWebshop Support">our support site</a></p>
    
    <h4>Manual PaymentProvider.config setup:</h4>
    <pre>
        <provider title="SagePay">
            <VendorName>uWebshop</VendorName>
            <Services>
              <Service name="CreditCard">
                <Issuers>
                  <Issuer>
                    <Code>VISA</Code>
                    <Name>VISA</Name>
                  </Issuer>
                </Issuers>
              </Service>
            </Services>
          </provider>
    </pre>    

	<asp:Label runat="server" ID="lblSagePayVendorName" AssociatedControlID="txtSagePayVendorName" Text="Your SagePay VendorName: "/>
	<asp:TextBox runat="server" ID="txtSagePayVendorName"/>
	<asp:Button runat="server" ID="btnInstall" OnClick="InstallConfig" Text="Install"/>
</div>