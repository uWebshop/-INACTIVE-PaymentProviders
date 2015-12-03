using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml.Linq;
using iDeal;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using uWebshop.API;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;
using uWebshop.Domain.Helpers;
using Issuer = iDeal.Directory.Issuer;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.IngAdvanced
{
	public class IngAdvancedPaymentProvider : IngAdvancedPaymentBase, IPaymentProvider
	{
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.ServerPost;
		}

	    public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
	    {
			var paymentProviderMethodList = new List<PaymentProviderMethod>();
			
			var issuers = HttpContext.Current.Cache["Issuers"] as IList<Issuer>;
			if (issuers == null)
			{
				var iDealService = new iDealService();
				var directoryResponse = iDealService.SendDirectoryRequest();
				issuers = directoryResponse.Issuers;
				
				//issuerss should only be requested once a day
				HttpContext.Current.Cache.Add("Issuers", issuers, null, DateTime.Now.AddDays(1), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			}

			paymentProviderMethodList.AddRange(issuers.Select(issuer => new PaymentProviderMethod
			{
				Id = issuer.Id.ToString(),
				ProviderName = GetName(),
				Title = issuer.Name,
				Description = issuer.ListType.ToString(),
			}));


			return paymentProviderMethodList;
        }
	}
}