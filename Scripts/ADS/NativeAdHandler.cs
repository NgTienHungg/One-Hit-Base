using TMPro;
using System;
using UnityEngine;
using com.adjust.sdk;
using UnityEngine.UI;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using OneHit.Internet;

namespace OneHit.ADS
{
     public abstract class NativeAdHandler : MonoBehaviour
     {
          private bool isRequesting;
          protected bool adReadyToShow;

          [Header("Key")]
          [SerializeField, GUIColor(0f, 0.8f, 1f)] private string androidNativeAdID;
          [SerializeField, GUIColor(0f, 0.8f, 1f)] private string iOSNativeAdID;

          [Header("Test")]
          [SerializeField] private bool testNativeAd;
          [ShowInInspector, ReadOnly] private string androidNativeAdTestID = "ca-app-pub-3940256099942544/2247696110";
          [ShowInInspector, ReadOnly] private string iOSNativeAdTestID = "ca-app-pub-3940256099942544/3986624511";

          private NativeAd _nativeAd;
          private string _adUnitId;

          #region Component
          [Header("Text")]
          [SerializeField] private TextMeshProUGUI adHeadline;
          [SerializeField] private TextMeshProUGUI adCallToAction;
          [SerializeField] private TextMeshProUGUI adBodyText;
          [SerializeField] private TextMeshProUGUI ratingAndPrice;

          [Header("Image")]
          [SerializeField] private Image adMainTexture;
          [SerializeField] private RawImage adIconTexture;
          [SerializeField] private RawImage infoIcon;

          private List<Texture2D> adTextures = new List<Texture2D>();
          private List<GameObject> adImages = new List<GameObject>();

          [Header("Waiting")]
          [SerializeField] public GameObject adLoaded;
          [SerializeField] public GameObject adLoading;
          #endregion

          protected virtual void Start()
          {
               if (AdsManager.IsAdsRemoved())
               {
                    gameObject.SetActive(false);
                    return;
               }
               if (adLoading != null)
               {
                    adLoading.SetActive(true);
               }
               else
               {
                    gameObject.SetActive(false);
               }
               if (adLoaded != null)
               {
                    adLoaded.SetActive(false);
               }
               MobileAds.Initialize(initStatus =>
               {
                    Debug.LogWarning("Native ad init complete!");
               });
               onAdLoaded += AdLoadedHandle;
               onAdFailedToLoad += AdFailedLoadHandle;
          }

          public abstract void TryShow();
          public abstract void AdLoadedHandle();
          public abstract void AdFailedLoadHandle();

          private Action onAdLoaded;
          private Action onAdFailedToLoad;

          public void RequestNativeAdHandle()
          {
               if (!InternetConnection.HasInternet()) return;
               if (isRequesting) return;
               isRequesting = true;
               Debug.Log(gameObject.name + " Request " + Time.time);
               RequestNativeAd();
          }

          private void RequestNativeAd()
          {
               Debug.LogWarning("Native ads: Loading ad...");
#if UNITY_ANDROID
               _adUnitId = testNativeAd || string.IsNullOrEmpty(androidNativeAdID)
                         ? androidNativeAdTestID
                         : androidNativeAdID;
#elif UNITY_IPHONE
          _adUnitId = testNativeAd || string.IsNullOrEmpty(iOSNativeAdID)
                    ? iOSNativeAdTestID
                    : iOSNativeAdID;
#endif

               AdLoader adLoader = new AdLoader.Builder(_adUnitId).ForNativeAd().Build();
               adLoader.OnNativeAdLoaded += HandleNativeAdLoaded;
               adLoader.OnAdFailedToLoad += HandleAdFailedToLoad;
               adLoader.LoadAd(new AdRequest.Builder().Build());
          }


          private void OnPaidEvent(object sender, AdValueEventArgs impressionData)
          {
               if (impressionData == null)
               {
                    Debug.LogError("Native ad: impression data is null!");
                    return;
               }

               Debug.LogWarning("Native ad: OnPaidEvent: " + impressionData);
               double revenue = impressionData.AdValue.Value / 1000000f;
               var imp = new[]  {
               new Parameter("ad_platform", "Admob"),
               new Parameter("ad_source", "Admob"),
               new Parameter("ad_unit_name", "native_ads"),
               new Parameter("ad_format", "native_ads"),
               new Parameter("value", revenue),
               new Parameter("currency", impressionData.AdValue.CurrencyCode)
           };
               FirebaseAnalytics.LogEvent("ad_impression", imp);

               AdjustAdRevenue adjustEvent = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
               //most important is calling setRevenue with two parameters
               adjustEvent.setRevenue(revenue, impressionData.AdValue.CurrencyCode);
               //Sent event to Adjust server
               Adjust.trackAdRevenue(adjustEvent);
          }

          private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
          {
               Debug.LogError(gameObject.name + " Native ad failed to load: " + args.LoadAdError.GetMessage());
               isRequesting = false;
               //RequestNativeAd();
               onAdFailedToLoad?.Invoke();
          }

          private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args)
          {
               adReadyToShow = true;
               isRequesting = false;

               if (gameObject.activeInHierarchy)
               {
                    FirebaseManager.LogEvent(FirebaseEvent.AD_NATIVE);
               }

               _nativeAd?.Destroy();
               _nativeAd = args.nativeAd;
               _nativeAd.OnPaidEvent += OnPaidEvent;

               #region Set Data To Component
               if (adIconTexture != null)
               {
                    adIconTexture.texture = _nativeAd.GetIconTexture();
                    if (!_nativeAd.RegisterIconImageGameObject(adIconTexture.gameObject))
                    {
                         Debug.LogError("Native ad: error registering icon");
                    }
               }

               if (infoIcon != null)
               {
                    infoIcon.texture = _nativeAd.GetAdChoicesLogoTexture();
                    infoIcon.SetNativeSize();
                    if (!_nativeAd.RegisterAdChoicesLogoGameObject(infoIcon.gameObject))
                    {
                         Debug.LogError("Native ad: error registering ad choices");
                    }
               }

               if (adHeadline != null)
               {
                    adHeadline.text = _nativeAd.GetHeadlineText();
                    if (!_nativeAd.RegisterHeadlineTextGameObject(adHeadline.gameObject))
                    {
                         Debug.LogError("Native ad: error registering headline");
                    }
               }

               if (adBodyText != null)
               {
                    adBodyText.text = _nativeAd.GetBodyText();
                    adBodyText.text = Compact(adBodyText.text);
                    if (!_nativeAd.RegisterBodyTextGameObject(adBodyText.gameObject))
                    {
                         Debug.LogError("Native ad: error registering body");
                    }
               }

               if (ratingAndPrice != null)
               {
                    ratingAndPrice.text = _nativeAd.GetStarRating() + "    " + _nativeAd.GetPrice();
                    if (Math.Abs(_nativeAd.GetStarRating() - (-1f)) < 0.1f)
                    {
                         ratingAndPrice.text = "";
                    }
                    if (!_nativeAd.RegisterPriceGameObject(ratingAndPrice.gameObject))
                    {
                         Debug.LogError("Native ad: error registering price");
                    }
               }

               if (adCallToAction != null)
               {
                    adCallToAction.text = _nativeAd.GetCallToActionText();
                    if (!_nativeAd.RegisterCallToActionGameObject(adCallToAction.gameObject))
                    {
                         Debug.LogError("Native ad: error registering call to action");
                    }
               }


               adTextures = _nativeAd.GetImageTextures();
               if (adTextures != null)
               {
                    var tex = adTextures.Count == 0 ? _nativeAd.GetIconTexture() : adTextures[0];
                    if (adMainTexture != null)
                    {
                         adImages.Add(adMainTexture.gameObject);
                         adImages[0].GetComponent<Image>().sprite = Sprite.Create(tex,
                             new Rect(0.0f, 0.0f, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f), 100.0f);

                         if (_nativeAd.RegisterImageGameObjects(adImages) == 0)
                         {
                              Debug.Log("error registering image");
                         }
                    }
               }
               #endregion

               onAdLoaded?.Invoke();

               if (adLoaded != null) adLoaded.SetActive(true);
               if (adLoading != null) adLoading.SetActive(false);
          }

          private string Compact(string s)
          {
               try
               {
                    if (s.Length < 75) return s;
                    s = s.Substring(0, 75);
                    s += "...";
                    return s;
               }
               catch (Exception e)
               {
                    Debug.LogError(e);
                    return "";
               }
          }
     }
}