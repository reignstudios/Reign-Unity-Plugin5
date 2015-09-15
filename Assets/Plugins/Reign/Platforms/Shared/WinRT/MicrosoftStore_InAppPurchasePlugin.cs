#if REIGN_POSTBUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using UnityEngine;
using Windows.UI.Core;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Xml.Serialization;
using System.IO;

#if WINDOWS_PHONE
using CurrentAppSimulator = MockIAPLib.CurrentApp;
#endif

namespace Reign.Plugin
{
	#if UNITY_METRO
	namespace InAppTestObjects
	{
		public class ListingInformation_MarketData
		{
			[XmlAttribute("xml:lang")] public string Lang;
			[XmlElement("Name")] public string Name;
			[XmlElement("Description")] public string Description;
			[XmlElement("Price")] public string Price;
			[XmlElement("CurrencySymbol")] public string CurrencySymbol;
			[XmlElement("CurrencyCode")] public string CurrencyCode;

			public ListingInformation_MarketData()
			{
				Lang = "en-us";
				Name = "In-app purchases";
				Description = "AppDescription";
				Price = "5.99";
				CurrencySymbol = "$";
				CurrencyCode = "USD";
			}
		}

		public class ListingInformation_App
		{
			[XmlElement("AppId")] public string AppId;
			[XmlElement("LinkUri")] public string LinkUri;
			[XmlElement("CurrentMarket")] public string CurrentMarket;
			[XmlElement("AgeRating")] public int AgeRating;

			[XmlElement("MarketData")] public ListingInformation_MarketData MarketData;

			public ListingInformation_App()
			{
				AppId = "988b90e4-5d4d-4dea-99d0-e423e414ffbc";
				LinkUri = "http://apps.microsoft.com/webpdp/app/988b90e4-5d4d-4dea-99d0-e423e414ffbc";
				CurrentMarket = "en-us";
				AgeRating = 3;
				MarketData = new ListingInformation_MarketData();
			}
		}

		public class ListingInformation_Product
		{
			[XmlAttribute("ProductId")] public string ProductId;
			[XmlAttribute("ProductType")] public string ProductType;
			[XmlElement("MarketData")] public ListingInformation_MarketData MarketData;

			public ListingInformation_Product()
			{
				MarketData = new ListingInformation_MarketData();
			}
		}

		public class ListingInformation
		{
			[XmlElement("App")] public ListingInformation_App App;
			[XmlElement("Product")] public ListingInformation_Product[] Products;

			public ListingInformation()
			{
				App = new ListingInformation_App();
			}
		}

		public class LicenseInformation_App
		{
			[XmlElement("IsActive")] public bool IsActive;
			[XmlElement("IsTrial")] public bool IsTrial;

			public LicenseInformation_App()
			{
				IsActive = true;
				IsTrial = false;
			}
		}

		public class LicenseInformation_Product
		{
			[XmlAttribute("ProductId")] public string ProductId;
			[XmlElement("IsActive")] public bool IsActive;

			public LicenseInformation_Product()
			{
				IsActive = false;
			}
		}

		public class LicenseInformation
		{
			[XmlElement("App")] public LicenseInformation_App App;
			[XmlElement("Product")] public LicenseInformation_Product[] Products;

			public LicenseInformation()
			{
				App = new LicenseInformation_App();
			}
		}

		[XmlRoot("CurrentApp")]
		public class CurrentApp
		{
			[XmlElement("ListingInformation")] public ListingInformation ListingInformation;
			[XmlElement("LicenseInformation")] public LicenseInformation LicenseInformation;

			public CurrentApp()
			{
				ListingInformation = new ListingInformation();
				LicenseInformation = new LicenseInformation();
			}
		}
	}
	#endif

	public class MicrosoftStore_InAppPurchasePlugin_Native : IInAppPurchasePlugin
	{
		private bool testTrialMode;
		public bool IsTrial
		{
			get
			{
				if (testTrialMode) return true;
				#if WINDOWS_PHONE
				if (testing) return wp8TestLicenseInformation.IsTrial;
				else return licenseInformation.IsTrial;
				#else
				return licenseInformation.IsTrial;
				#endif
			}
		}

		public InAppPurchaseID[] InAppIDs {get; private set;}
		private bool testing;
		private LicenseInformation licenseInformation;
		#if WINDOWS_PHONE
		private MockIAPLib.LicenseInformation wp8TestLicenseInformation;
		private MockIAPLib.ListingInformation wp8TestListingInformation;
		#endif

		public MicrosoftStore_InAppPurchasePlugin_Native(InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback)
		{
			testing = desc.Testing;
			testTrialMode = desc.TestTrialMode;

			#if WINDOWS_PHONE
			InAppIDs = desc.WP8_MicrosoftStore_InAppIDs;
			#else
			InAppIDs = desc.WinRT_MicrosoftStore_InAppIDs;
			#endif

			if (desc.Testing)
			{
				loadTestData(desc, createdCallback);
			}
			else
			{
				licenseInformation = CurrentApp.LicenseInformation;
				licenseInformation.LicenseChanged += licenseChanged;
				if (createdCallback != null) createdCallback(true);
			}
		}
		
		#if WINDOWS_PHONE
		private async void loadTestData(InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback)
		{
			bool pass = true;
			try
			{
				MockIAPLib.MockIAP.Init();
				MockIAPLib.MockIAP.RunInMockMode(true);
				MockIAPLib.MockIAP.SetListingInformation(1, "en-us", "Some description", "1", "TestApp");

				int i = 1;
				foreach (var inApp in desc.WP8_MicrosoftStore_InAppIDs)
				{
					var product = new MockIAPLib.ProductListing
					{
						Name = inApp.ID,
						ImageUri = new Uri("/Res/ReignIcon.png", UriKind.Relative),
						ProductId = inApp.ID,
						ProductType = inApp.Type == InAppPurchaseTypes.NonConsumable ? ProductType.Durable : ProductType.Consumable,
						Keywords = new string[] {"image"},
						Description = "Product Desc " + i,
						FormattedPrice = inApp.CurrencySymbol + inApp.Price,
						Tag = string.Empty
					};

					MockIAPLib.MockIAP.AddProductListing(inApp.ID, product);
					++i;
				}

				wp8TestListingInformation = await CurrentAppSimulator.LoadListingInformationAsync();
				MockIAPLib.MockIAP.ClearCache();
				wp8TestLicenseInformation = CurrentAppSimulator.LicenseInformation;
				wp8TestLicenseInformation.LicenseChanged += licenseChanged;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}

			if (createdCallback != null) createdCallback(pass);
		}
		#else
		private async void loadTestData(InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback)
		{
			bool pass = true;
			try
			{
				// create xml obj
				var currentApp = new InAppTestObjects.CurrentApp();
				currentApp.ListingInformation.Products = new InAppTestObjects.ListingInformation_Product[desc.WinRT_MicrosoftStore_InAppIDs.Length];
				currentApp.LicenseInformation.Products = new InAppTestObjects.LicenseInformation_Product[desc.WinRT_MicrosoftStore_InAppIDs.Length];
				for (int i = 0; i != currentApp.ListingInformation.Products.Length; ++i)
				{
					var listingProduct = new InAppTestObjects.ListingInformation_Product();
					listingProduct.ProductId = desc.WinRT_MicrosoftStore_InAppIDs[i].ID;
					listingProduct.ProductType = desc.WinRT_MicrosoftStore_InAppIDs[i].Type == InAppPurchaseTypes.NonConsumable ? null : "Consumable";
					listingProduct.MarketData.Name = desc.WinRT_MicrosoftStore_InAppIDs[i].ID;
					listingProduct.MarketData.Description = null;
					listingProduct.MarketData.Price = desc.WinRT_MicrosoftStore_InAppIDs[i].Price.ToString();
					listingProduct.MarketData.CurrencySymbol = desc.WinRT_MicrosoftStore_InAppIDs[i].CurrencySymbol;
					currentApp.ListingInformation.Products[i] = listingProduct;

					var licenseProduct = new InAppTestObjects.LicenseInformation_Product();
					licenseProduct.ProductId = desc.WinRT_MicrosoftStore_InAppIDs[i].ID;
					currentApp.LicenseInformation.Products[i] = licenseProduct;
				}

				// serialize obj
				var xml = new XmlSerializer(typeof(InAppTestObjects.CurrentApp));
				byte[] data = null;
				using (var stream = new MemoryStream())
				{
					xml.Serialize(stream, currentApp);
					stream.Position = 0;
					data = new byte[stream.Length];
					stream.Read(data, 0, data.Length);
				}

				// write and read test InApp data
				StorageFile writeFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("TEST_InAppPurchase.xml", CreationCollisionOption.ReplaceExisting);
				await FileIO.WriteBytesAsync(writeFile, data);
			
				StorageFile readFile = await ApplicationData.Current.LocalFolder.GetFileAsync("TEST_InAppPurchase.xml");
				await CurrentAppSimulator.ReloadSimulatorAsync(readFile);
				licenseInformation = CurrentAppSimulator.LicenseInformation;
				licenseInformation.LicenseChanged += licenseChanged;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				pass = false;
			}

			if (createdCallback != null) createdCallback(pass);
		}
		#endif

		private void licenseChanged()
		{
			Debug.Log("InAppPurchasePlugin: LicenseChanged");
		}

		public async void GetProductInfo(InAppPurchaseGetProductInfoCallbackMethod callback)
		{
			if (callback == null) return;

			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(async delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			#endif
			{
				var infos = new List<InAppPurchaseInfo>();
				try
				{
					#if WINDOWS_PHONE
					if (testing)
					{
						var listingInfo = wp8TestListingInformation;
						foreach (var l in listingInfo.ProductListings)
						{
							var info = new InAppPurchaseInfo()
							{
								ID = l.Value.ProductId,
								FormattedPrice = l.Value.FormattedPrice
							};
							infos.Add(info);
						}
					}
					else
					{
						var listingInfo = await CurrentApp.LoadListingInformationAsync();
						foreach (var l in listingInfo.ProductListings)
						{
							var info = new InAppPurchaseInfo()
							{
								ID = l.Value.ProductId,
								FormattedPrice = l.Value.FormattedPrice
							};
							infos.Add(info);
						}
					}
					#else
					ListingInformation listingInfo;
					if (testing) listingInfo = await CurrentAppSimulator.LoadListingInformationAsync();
					else listingInfo = await CurrentApp.LoadListingInformationAsync();
					foreach (var l in listingInfo.ProductListings)
					{
						var info = new InAppPurchaseInfo()
						{
							ID = l.Value.ProductId,
							FormattedPrice = l.Value.FormattedPrice
						};
						infos.Add(info);
					}
					#endif

					callback(infos.ToArray(), true);
				}
				catch (Exception e)
				{
					Debug.LogError(e.Message);
					callback(null, false);
				}
			});
		}

		public void Restore(InAppPurchaseRestoreCallbackMethod restoreCallback)
		{
			if (restoreCallback == null) return;
			restoreAsync(restoreCallback);
		}

		#if WINDOWS_PHONE
		private void restoreAsync(InAppPurchaseRestoreCallbackMethod restoreCallback)
		#else
		private async void restoreAsync(InAppPurchaseRestoreCallbackMethod restoreCallback)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate()
			#endif
			{
				foreach (var inAppID in InAppIDs)
				{
					#if WINDOWS_PHONE
					if (testing)
					{
						var testProductID = wp8TestListingInformation.ProductListings[inAppID.ID].ProductId;
						restoreCallback(inAppID.ID, wp8TestLicenseInformation.ProductLicenses[testProductID].IsActive && !wp8TestLicenseInformation.ProductLicenses[testProductID].IsConsumable);
					}
					else
					{
						restoreCallback(inAppID.ID, licenseInformation.ProductLicenses[inAppID.ID].IsActive && !licenseInformation.ProductLicenses[inAppID.ID].IsConsumable);
					}
					#else
					restoreCallback(inAppID.ID, licenseInformation.ProductLicenses[inAppID.ID].IsActive && inAppID.Type != InAppPurchaseTypes.Consumable);
					#endif
				}
			});
		}

		public void BuyInApp(string inAppID, InAppPurchaseBuyCallbackMethod purchasedCallback)
		{
			buyInAppAsync(inAppID, purchasedCallback);
		}

		#if WINDOWS_PHONE
		private void buyInAppAsync(string inAppID, InAppPurchaseBuyCallbackMethod purchasedCallback)
		#else
		private async void buyInAppAsync(string inAppID, InAppPurchaseBuyCallbackMethod purchasedCallback)
		#endif
		{
			#if WINDOWS_PHONE
			WinRTPlugin.Dispatcher.BeginInvoke(async delegate()
			#else
			await WinRTPlugin.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async delegate()
			#endif
			{
				#if WINDOWS_PHONE
				if ((testing && (!wp8TestLicenseInformation.ProductLicenses[inAppID].IsActive || wp8TestLicenseInformation.ProductLicenses[inAppID].IsConsumable)) ||
					(!testing && (!licenseInformation.ProductLicenses[inAppID].IsActive || licenseInformation.ProductLicenses[inAppID].IsConsumable)))
				#else
				if (!licenseInformation.ProductLicenses[inAppID].IsActive || isConsumbable(inAppID))
				#endif
				{
					try
					{
						string receipt = null;
						#if WINDOWS_PHONE
						string productID = null;
						if (testing) productID = wp8TestListingInformation.ProductListings[inAppID].ProductId;
						else productID = licenseInformation.ProductLicenses[inAppID].ProductId;
						#elif UNITY_METRO_8_0
						string productID = licenseInformation.ProductLicenses[inAppID].ProductId;
						#else
						PurchaseResults results;
						string productID = licenseInformation.ProductLicenses[inAppID].ProductId;
						#endif

						if (testing)
						{
							#if WINDOWS_PHONE
							receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(productID, true);
							if (wp8TestLicenseInformation.ProductLicenses[inAppID].IsActive)
							{
								PlayerPrefsEx.SetIntAsync("ReignIAP_PurchasedAwarded_" + inAppID, 0, true);
							}
							#elif UNITY_METRO_8_0
							receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(productID, true);
							#else
							results = await CurrentAppSimulator.RequestProductPurchaseAsync(productID);
							receipt = results.ReceiptXml;
							#endif
						}
						else
						{
							#if WINDOWS_PHONE
							receipt = await CurrentApp.RequestProductPurchaseAsync(productID, true);
							if (licenseInformation.ProductLicenses[inAppID].IsActive)
							{
								PlayerPrefsEx.SetIntAsync("ReignIAP_PurchasedAwarded_" + inAppID, 0, true);
							}
							#elif UNITY_METRO_8_0
							receipt = await CurrentApp.RequestProductPurchaseAsync(productID, true);
							#else
							results = await CurrentApp.RequestProductPurchaseAsync(productID);
							receipt = results.ReceiptXml;
							#endif
						}
						
						#if UNITY_METRO_8_0
						if (!string.IsNullOrEmpty(receipt) || licenseInformation.ProductLicenses[inAppID].IsActive)
						{
							PlayerPrefsEx.SetIntAsync("ReignIAP_PurchasedAwarded_" + inAppID, 0, true);
						}
						#elif UNITY_METRO
						if (results.Status == ProductPurchaseStatus.Succeeded || results.Status == ProductPurchaseStatus.AlreadyPurchased || licenseInformation.ProductLicenses[inAppID].IsActive)
						{
							PlayerPrefsEx.SetIntAsync("ReignIAP_PurchasedAwarded_" + inAppID, 0, true);
						}
						#endif
						
						if (purchasedCallback != null)
						{
							#if WINDOWS_PHONE
							if (testing)
							{
								purchasedCallback(inAppID, receipt, wp8TestLicenseInformation.ProductLicenses[inAppID].IsActive);
								if (wp8TestLicenseInformation.ProductLicenses[inAppID].IsConsumable) CurrentAppSimulator.ReportProductFulfillment(productID);
							}
							else
							{
								purchasedCallback(inAppID, receipt, licenseInformation.ProductLicenses[inAppID].IsActive);
								if (licenseInformation.ProductLicenses[inAppID].IsConsumable) CurrentApp.ReportProductFulfillment(productID);
							}
							#elif UNITY_METRO_8_0
							purchasedCallback(inAppID, receipt, !string.IsNullOrEmpty(receipt) || licenseInformation.ProductLicenses[inAppID].IsActive);
							if (isConsumbable(inAppID))
							{
								Debug.LogError("NOTE: Consumable IAP not supported in 8.0");
							}
							#else
							purchasedCallback(inAppID, receipt, results.Status == ProductPurchaseStatus.Succeeded || results.Status == ProductPurchaseStatus.AlreadyPurchased || licenseInformation.ProductLicenses[inAppID].IsActive);
							if (isConsumbable(inAppID))
							{
								if (testing) await CurrentAppSimulator.ReportConsumableFulfillmentAsync(productID, results.TransactionId);
								else await CurrentApp.ReportConsumableFulfillmentAsync(productID, results.TransactionId);
							}
							#endif
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e.Message);
						if (purchasedCallback != null) purchasedCallback(inAppID, null, false);
					}
				}
				else
				{
					if (purchasedCallback != null) purchasedCallback(inAppID, null, true);
				}
			});
		}

		#if UNITY_METRO
		private bool isConsumbable(string inAppID)
		{
			foreach (var id in InAppIDs)
			{
				if (id.ID == inAppID && id.Type == InAppPurchaseTypes.Consumable)
				{
					return true;
				}
			}

			return false;
		}
		#endif

		public void Update()
		{
			// do nothing...
		}
	}
}
#elif UNITY_WINRT
namespace Reign.Plugin
{
	public class MicrosoftStore_InAppPurchasePlugin_WinRT : IInAppPurchasePlugin
	{
		public IInAppPurchasePlugin Native;

		public delegate void InitNativeMethod(MicrosoftStore_InAppPurchasePlugin_WinRT plugin, InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback);
		public static InitNativeMethod InitNative;

		public MicrosoftStore_InAppPurchasePlugin_WinRT(InAppPurchaseDesc desc, InAppPurchaseCreatedCallbackMethod createdCallback)
		{
			InitNative(this, desc, createdCallback);
		}

		public bool IsTrial
		{
			get {return Native.IsTrial;}
		}

		public InAppPurchaseID[] InAppIDs
		{
			get {return Native.InAppIDs;}
		}

		public void GetProductInfo(InAppPurchaseGetProductInfoCallbackMethod callback)
		{
			Native.GetProductInfo(callback);
		}

		public void Restore(InAppPurchaseRestoreCallbackMethod restoreCallback)
		{
			Native.Restore(restoreCallback);
		}

		public void BuyInApp(string inAppID, InAppPurchaseBuyCallbackMethod purchasedCallback)
		{
			Native.BuyInApp(inAppID, purchasedCallback);
		}

		public void Update()
		{
			Native.Update();
		}
	}
}
#endif