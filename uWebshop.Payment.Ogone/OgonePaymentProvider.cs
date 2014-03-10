using System.Collections.Generic;
using System.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;
using umbraco;

namespace uWebshop.Payment.Ogone
{
	public class OgonePaymentProvider : OgonePaymentBase, IPaymentProvider
	{
		#region IPaymentProvider Members

		PaymentTransactionMethod IPaymentProvider.GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
		{
			
			var paymentMethods = new List<PaymentProviderMethod>();

			// the payment methods can be created as nodes in the Tree
			// link: https://secure.ogone.com/Ncol/Test/PM_process_procedure.asp?CSRFSP=%2fncol%2ftest%2fbackoffice%2fsupportgetdownloaddocument.asp&CSRFKEY=0BA884EF910B4C61C38ADA7C2B6E2DC445408AF9&CSRFTS=20140203134617&branding=OGONE&MigrationMode=DOTNET
			// uWebshop uses the title field on th payment provider method node
			// format the title like this: PMvalue|BRANDvalue
			// example: CreditCard|MasterCard
			// you can also make them fixed by adding them to this method.

			//paymentMethods.Add(
			//	new PaymentProviderMethod
			//	{
			//		Id = "CreditCard|MasterCard",
			//		ProviderName = GetName(),
			//		Title = "CreditCard|MasterCard",
			//		Description = "MasterCard"
			//	});

			return paymentMethods;
		}

		#endregion
	}
}