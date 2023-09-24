using System;
using OneHit.ADS;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Purchasing;
using System.Collections.Generic;

namespace OneHit.IAP
{
     public class Purchaser : Singleton<Purchaser>, IStoreListener
     {
          [Space, InfoBox("The first product must be <color=yellow> remove_ads </color>", InfoMessageType.Warning)]
          public ProductInfo[] productsInfo;

          private IStoreController _storeController;
          private IExtensionProvider _storeExtensionProvider;
          private IAppleExtensions _appleExtensions;

          private List<string> _prices = new List<string>();


          #region ========== INITIALIZATION ==========

          private void Start()
          {
               // If we haven't set up the Unity Purchasing reference
               if (_storeController == null)
               {
                    InitializePurchasing();
               }
               // await new WaitUntil(() => CodelessIAPStoreListener.initializationComplete);
          }

          private void InitializePurchasing()
          {
               // If we have already connected to Purchasing ...
               if (IsInitialized())
               {
                    return;
               }

#if UNITY_EDITOR
               Debug.Log("<color=yellow> [Purchaser]: use FakeStoreUIMode </color>");
               StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif

               Debug.Log("<color=orange> [Purchaser]: start init... </color>");
               var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
               foreach (var product in productsInfo)
               {
                    builder.AddProduct(product.ID, product.type);
               }
               UnityPurchasing.Initialize(CodelessIAPStoreListener.Instance, builder);
          }

          private bool IsInitialized()
          {
               // Only say we are initialized if both the Purchasing references are set.
               return _storeController != null && _storeExtensionProvider != null;
          }

          public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
          {
               // Purchasing has succeeded initializing. Collect our Purchasing references.
               Debug.LogWarning("[Puchaser]: Initialized complete!");

               _storeController = controller;
               _storeExtensionProvider = extensions;
               _appleExtensions = CodelessIAPStoreListener.Instance.GetStoreExtensions<IAppleExtensions>();

               // kiểm tra xem User đã mua gói RemoveAds nhưng không thành công và được hoàn tiền
               CheckRefundRemoveAds();
               CheckSubscription();
          }

          public void OnInitializeFailed(InitializationFailureReason error)
          {
               // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
               Debug.LogError($"[Purchaser]: Initialization Failure Reason: {error}");
          }

          public void OnInitializeFailed(InitializationFailureReason error, string message)
          {
               // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
               Debug.LogError($"[Purchaser]: Initialization Failure Reason: {error}");
          }

          public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
          {
               return PurchaseProcessingResult.Complete;
          }

          public void BuyProductID(string productId)
          {
               if (IsInitialized())
               {
                    var product = _storeController.products.WithID(productId);

                    if (product != null && product.availableToPurchase)
                    {
                         Debug.LogWarning(string.Format("[Purchaser]: purchasing product asychronously: '{0}'", product.definition.id));
                         _storeController.InitiatePurchase(product);
                    }
                    else
                    {
                         Debug.LogError("[Purchaser]: BuyProductID FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
               }
               else
               {
                    Debug.LogError("[Purchaser]: BuyProductID FAIL. Not initialized.");
               }
          }

          public void RestorePurchases()
          {
               // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
               // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.

               if (!IsInitialized())
               {
                    Debug.LogError("[Purchaser]: RestorePurchases FAIL. Not initialized.");
                    return;
               }

               #region use in-app-purchase 4.5.2 or older version
               if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
               {
                    Debug.Log("[Purchaser]: RestorePurchases started ...");

                    // Fetch the Apple store-specific subsystem.
                    var apple = CodelessIAPStoreListener.Instance.GetStoreExtensions<IAppleExtensions>();

                    // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                    // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                    apple.RestoreTransactions((result, message) =>
                    {
                         // The first phase of restoration. If no more responses are received
                         // on ProcessPurchase then no purchases are available to be restored.
                         Debug.LogWarning("[Puchaser]: Restore purchases continue: " + result + ". If no further messages, no purchases available to restore.");
                    });
               }
               else
               {
                    Debug.LogError("[Puchaser]: RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
               }
               #endregion
          }

          #endregion

          #region ========== UTILS ==========
          public bool HasReceipt(string id)
          {
               if (_storeController != null)
               {
                    if (_storeController.products.WithID(id).hasReceipt)
                    {
                         return true;
                    }
               }
               return false;
          }

          public List<string> GetPrices()
          {
               foreach (var product in _storeController.products.all)
               {
                    Debug.Log(product.metadata.localizedTitle + " " + product.metadata.localizedPriceString + " " + product.metadata.localizedPrice);
                    _prices.Add(product.metadata.localizedPriceString);
               }
               return _prices;
          }

          public decimal GetPrice(int id)
          {
               try
               {
                    return _storeController.products.all[id].metadata.localizedPrice;
               }
               catch (Exception e)
               {
                    Debug.Log(e.ToString());
                    return 0;
               }
          }
          #endregion

          #region ========== Subscription ==========
          public bool CheckSubscription()
          {
               Dictionary<string, string> introductory_info_dict = _appleExtensions.GetIntroductoryPriceDictionary();
               // Sample code for expose product sku details for apple store
               //Dictionary<string, string> product_details = m_AppleExtensions.GetProductDetails();

               Debug.Log("Available items:");
               foreach (var item in _storeController.products.all)
               {
                    if (item.availableToPurchase)
                    {
                         Debug.Log(string.Join(" - ", new[] {
                              item.metadata.localizedTitle,
                              item.metadata.localizedDescription,
                              item.metadata.isoCurrencyCode,
                              item.metadata.localizedPrice.ToString(),
                              item.metadata.localizedPriceString,
                              item.transactionID,
                              item.receipt
                         }));

#if INTERCEPT_PROMOTIONAL_PURCHASES
                // Set all these products to be visible in the user's App Store according to Apple's Promotional IAP feature
                // https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/StoreKitGuide/PromotingIn-AppPurchases/PromotingIn-AppPurchases.html
                m_AppleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
#endif
                         // this is the usage of SubscriptionManager class
                         if (item.receipt != null)
                         {
                              if (item.definition.type == ProductType.Subscription)
                              {
                                   Debug.Log("CHECK SUBSCRIPTION :" + item.metadata.localizedTitle + " :: " + CheckIfProductIsAvailableForSubscriptionManager(item.receipt));
                                   if (CheckIfProductIsAvailableForSubscriptionManager(item.receipt))
                                   {
                                        string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
                                        SubscriptionManager p = new SubscriptionManager(item, intro_json);
                                        SubscriptionInfo info = p.getSubscriptionInfo();
                                        Debug.Log("product id is: " + info.getProductId());
                                        Debug.Log("purchase date is: " + info.getPurchaseDate());
                                        Debug.Log("subscription next billing date is: " + info.getExpireDate());
                                        Debug.Log("is subscribed? " + info.isSubscribed().ToString());
                                        Debug.Log("is expired? " + info.isExpired().ToString());
                                        Debug.Log("is cancelled? " + info.isCancelled());
                                        Debug.Log("product is in free trial peroid? " + info.isFreeTrial());
                                        Debug.Log("product is auto renewing? " + info.isAutoRenewing());
                                        Debug.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                                        Debug.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                                        Debug.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
                                        Debug.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                                        Debug.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
                                   }
                                   else
                                   {
                                        Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                                   }
                              }
                              else
                              {
                                   Debug.Log("the product is not a subscription product");
                              }
                         }
                         else
                         {
                              Debug.Log("the product should have a valid receipt " + item.definition.id);
                         }
                    }
               }
               return false;
          }

          private bool CheckIfProductIsAvailableForSubscriptionManager(string receipt)
          {
               var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
               if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
               {
                    Debug.Log("The product receipt does not contain enough information");
                    return false;
               }
               var store = (string)receipt_wrapper["Store"];
               var payload = (string)receipt_wrapper["Payload"];

               if (payload != null)
               {
                    switch (store)
                    {
                         case GooglePlay.Name:
                         {
                              var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                              if (!payload_wrapper.ContainsKey("json"))
                              {
                                   Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                                   return false;
                              }
                              var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                              if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
                              {
                                   Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                                   return false;
                              }
                              var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                              var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                              if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
                              {
                                   Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                                   return false;
                              }
                              return true;
                         }
                         case AppleAppStore.Name:
                         case AmazonApps.Name:
                         case MacAppStore.Name:
                         {
                              return true;
                         }
                         default:
                         {
                              return false;
                         }
                    }
               }
               return false;
          }
          #endregion

          private void CheckRefundRemoveAds()
          {
               Debug.LogWarning("[Purchaser]: start check refund...");

               string keyRemoveAds = productsInfo[0].ID;
               bool hasReceiptRemoveAds = _storeController.products.WithID(keyRemoveAds).hasReceipt;
               Debug.Log($"Product: {keyRemoveAds} - {hasReceiptRemoveAds}");

               if (!hasReceiptRemoveAds && AdsManager.IsAdsRemoved())
               {
                    Debug.LogWarning("[Purchaser]: buy pack RemoveAds not success => Reset ads!");
                    AdsManager.SetAdsRemoved(false);
               }
          }

          public void CheckRestore()
          {
               Debug.LogWarning("[Purchaser]: start check restore...");

               if (_storeController != null)
               {
                    string keyRemoveAds = productsInfo[0].ID;
                    bool hasReceiptRemoveAds = _storeController.products.WithID(keyRemoveAds).hasReceipt;
                    Debug.Log($"Product: {keyRemoveAds} - {hasReceiptRemoveAds}");

                    if (hasReceiptRemoveAds)
                    {
                         OnRestore(keyRemoveAds);
                         //NotificationPanel.Instance.Enable("NOTIFICATION", "Restore success!");
                    }
                    else
                    {
                         //NotificationPanel.Instance.Enable("NOTIFICATION", "Nothing to restore!");
                    }
               }
          }


          //--------------------------------------------------//


          public void OnPurchased(Product product)
          {
               Debug.LogWarning($"<color=lime> [Purchaser]: OnPurchased {product.definition.id} </color>");
               string productID = product.definition.id;

               if (productID.Equals(productsInfo[0].ID))
               {
                    FirebaseManager.LogEvent(FirebaseEvent.PURCHASE_SUCCESS_NOADS);
                    MasterControl.Instance.HideBanner();
                    AdsManager.SetAdsRemoved(true);
               }
               //else if (productID.Equals(productsInfo[1].ID)) {
               //      TODO
               //}
               //else if (productID.Equals(productsInfo[2].ID)) {
               //      TODO
               //}
          }

          public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
          {
               // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
               // this reason with the user to guide their troubleshooting actions.
               Debug.LogError(string.Format("[Purchaser]: OnPurchaseFailed: Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

               //! Nếu đã mua RemoveAds => Restore
               if (failureReason.Equals(PurchaseFailureReason.DuplicateTransaction))
               {
                    if (product != null)
                    {
                         string productID = product.definition.id;
                         if (productID.Equals(productsInfo[0].ID) && HasReceipt(productID))
                         {
                              OnRestore(productID);
                         }
                    }
               }
          }

          private void OnRestore(string productID)
          {
               if (productID.Equals(productsInfo[0].ID))
               {
                    MasterControl.Instance.HideBanner();
                    AdsManager.SetAdsRemoved(true);
               }
          }
     }
}