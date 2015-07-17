using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using SagePay.IntegrationKit;

namespace uWebshop.Payment.SagePay
{
	public class SagePayConfiguration : ConfigurationSection
	{
		public static SagePayConfiguration GetConfig()
		{
			return (SagePayConfiguration) ConfigurationManager.GetSection("SagePayConfiguration");
		}


		[ConfigurationProperty("", IsDefaultCollection = true)]
		public SagePayConfigurationCollection Settings
		{
			get { return (SagePayConfigurationCollection) base[""]; }
		}
	}

	[ConfigurationCollection(typeof (SagePayConfigurationElement))]
	public class SagePayConfigurationCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new SagePayConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SagePayConfigurationElement) (element)).Key;
		}

		public SagePayConfigurationElement this[int idx]
		{
			get { return (SagePayConfigurationElement) BaseGet(idx); }
		}
	}

	public class SagePayConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("key", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Key
		{
			get { return ((string) (base["key"])); }
			set { base["key"] = value; }
		}

		[ConfigurationProperty("value", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string Value
		{
			get { return ((string) (base["value"])); }
			set { base["value"] = value; }
		}
	}
}