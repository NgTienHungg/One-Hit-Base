using System;
using UnityEngine;
using com.adjust.sdk;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace OneHit.ADS
{
     public class AppOpenAdHandler : MonoBehaviour, IAdHandler
     {
          private string _adUnitId;
          private float _loadAdWaitTime;

          private AppOpenAd _appOpenAd;
          private DateTime _adExpireTime;

          private bool IsAdAvailable => _appOpenAd != null
                                     && _appOpenAd.CanShowAd()
                                     && DateTime.Now < _adExpireTime;

          public void Init()
          {
               Logger.Warning("</AppOpen> is initializing...");

               _loadAdWaitTime = OneHitConfigs.appOpenAdLoadWaitTime;

#if UNITY_ANDROID
               _adUnitId = OneHitConfigs.adsMode == AdsMode.TEST || string.IsNullOrEmpty(OneHitConfigs.androidAppOpenAdId)
                         ? OneHitConfigs.androidAppOpenAdTestId
                         : OneHitConfigs.androidAppOpenAdId;
#elif UNITY_IPHONE
               _adUnitId = OneHitConfigs.adsMode == AdsMode.TEST || string.IsNullOrEmpty(OneHitConfigs.iOSAppOpenAdId)
                         ? OneHitConfigs.iOSAppOpenAdTestId
                         : OneHitConfigs.iOSAppOpenAdId;
#endif
          }

          public void Load()
          {
               // Clean up the old ad before loading a new one.
               if (_appOpenAd != null)
               {
                    _appOpenAd.Destroy();
                    _appOpenAd = null;
               }

               Logger.Warning("</AppOpen> is loading...");
               AppOpenAd.Load(_adUnitId, ScreenOrientation.Portrait, new AdRequest.Builder().Build(), (ad, error) =>
               {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                         Logger.Error("</AppOpen> App open ad failed to load an ad with error: " + error);
                         return;
                    }

                    // App open ad is loaded.
                    Logger.Warning("</AppOpen> App open ad loaded with response: " + ad.GetResponseInfo());
                    _adExpireTime = DateTime.Now + TimeSpan.FromHours(4); // App open ads can be preloaded for up to 4 hours.
                    _appOpenAd = ad;

                    RegisterAppOpenAdEvents();
               });
          }

          public async void Show()
          {
               Logger.Warning("</AppOpen> is showing...");

               if (!IsAdAvailable)
               {
                    Logger.Error("</AppOpen> not available, reload...");
                    AdsManager.Instance.LoadAppOpenAd();

                    Logger.Warning($"</AppOpen> wait until available (no more {_loadAdWaitTime}s)");
                    var timeOut = DateTime.Now.AddSeconds(_loadAdWaitTime);
                    await UniTask.WaitUntil(() => IsAdAvailable || DateTime.Now > timeOut);
               }

               if (IsAdAvailable)
               {
                    Logger.Warning("</AppOpen> ready to show");
                    _appOpenAd.Show();
               }
               else
               {
                    Logger.Error("</AppOpen> not ready to show");
                    AdsManager.Instance.OnAppOpenAdCallback(false);
               }
          }

          public void Hide() { }


          #region  =============== APP OPEN AD EVENTS ===============

          private void RegisterAppOpenAdEvents()
          {
               _appOpenAd.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
               _appOpenAd.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
               _appOpenAd.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
               _appOpenAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
               _appOpenAd.OnAdClicked += OnAdClicked;
               _appOpenAd.OnAdPaid += OnAdPaid;
          }

          // Raised when an ad opened full screen content
          private void OnAdFullScreenContentOpened()
          {
               Logger.Warning("</AppOpen> On Ad Full Screen Content Opened");
               //AdsManager.Instance.allowShowOpenAd = false;
               AdsManager.Instance.OnStartShowAd(true);
          }

          // Raised when the ad closed full screen content
          private void OnAdFullScreenContentClosed()
          {
               Logger.Warning("</AppOpen> On Ad Full Screen Content Closed");
               FirebaseManager.LogEvent(FirebaseEvent.AD_OPEN);
               AdsManager.Instance.OnAppOpenAdCallback(true);

               //AdsManager.Instance.allowShowOpenAd = true;
               AdsManager.Instance.OnEndOfShowAd(true);
               AdsManager.Instance.LoadAppOpenAd();
          }

          // Raised when the ad failed to open full screen content.
          private void OnAdFullScreenContentFailed(AdError error)
          {
               Logger.Error("</AppOpen> App open ad failed to open full screen content with error " + error.GetMessage());
               AdsManager.Instance.OnAppOpenAdCallback(false);
          }

          // Raised when a click is recorded for an ad.
          private void OnAdClicked()
          {
               Logger.Warning("</AppOpen> App open ad was clicked");
          }

          // Raised when an impression is recorded for an ad
          private void OnAdImpressionRecorded()
          {
               Logger.Warning("</AppOpen> App open ad recorded an impression");
          }

          // Raised when the ad is estimated to have earned money
          private void OnAdPaid(AdValue adValue)
          {
               Logger.Warning($"</AppOpen> App open ad paid value: {adValue.Value}, {adValue.CurrencyCode})");
               OnAppOpenAdPaid(adValue);
          }

          private void OnAppOpenAdPaid(AdValue adValue)
          {
               double revenue = adValue.Value / 1000000f;

               var impression = new[] {
                    new Parameter("ad_platform", "admob"),
                    new Parameter("ad_source", "admob"),
                    new Parameter("ad_unit_name", "open_ads"),
                    new Parameter("ad_format", "open_ads"),
                    new Parameter("value", revenue),
                    new Parameter("currency", adValue.CurrencyCode)
               };
               FirebaseAnalytics.LogEvent("ad_impression", impression);

               AdjustAdRevenue adjustEvent = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
               adjustEvent.setRevenue(revenue, adValue.CurrencyCode);
               Adjust.trackAdRevenue(adjustEvent);
          }

          #endregion


          #region  =============== APP STATE CHANGED ===============

          private void Awake()
          {
               // Use the AppStateEventNotifier to listen to application open/close events.
               AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
          }

          private void OnDestroy()
          {
               // Always unlisten to events when complete.
               AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
          }

          private void OnAppStateChanged(AppState state)
          {
               Logger.Warning("</AppOpen> App state changed to " + state);

               // if the app is Foregrounded and the ad is available, show it.
               if (state == AppState.Foreground)
               {
                    //! NOT SHOW OPEN AD BY THIS EVENT WHEN IN LOADING SCENE
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                         return;

                    AdsManager.Instance.ShowAppOpenAd(null);
               }
          }

          #endregion
     }
}