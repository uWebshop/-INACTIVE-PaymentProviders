using SagePay.IntegrationKit;
using SagePay.IntegrationKit.Messages;

namespace uWebshop.Payment.SagePay
{
	internal class SagePayDirectIntegration : SagePayAPIIntegration
	{
		public IDirectPaymentResult ProcessDirect3D(IThreeDAuthRequest request)
		{
			//request.TransactionType = TransactionType.three;
			RequestQueryString = BuildQueryString(request, ProtocolMessage.THREE_D_AUTH_REQUEST, SagePaySettings.ProtocolVersion);
			ResponseQueryString = ProcessWebRequestToSagePay(SagePaySettings.Direct3dSecureUrl, RequestQueryString);
			IDirectPaymentResult result = GetDirectPaymentResult(ResponseQueryString);
			return result;
		}


		public IThreeDAuthRequest ThreeDAuthRequest()
		{
			IThreeDAuthRequest request = new DataObject();
			return request;
		}


	}
}