using System;
using UnityEngine;
using com.adjust.sdk;
using OneHit.Internet;
using Firebase.Analytics;
using GoogleMobileAds.Api;

namespace OneHit.ADS
{
     public enum AdsMode
     {
          PRODUCTION,
          UNLOCK,
          TEST,
     }

     public class AdsManager : Singleton<AdsManager>
     {
          [SerializeField] private AppOpenAdHandler appOpenAdHandler;
          [SerializeField] private BannerAdHandler bannerAdHandler;
          [SerializeField] private InterstitialAdHandler interstitialAdHandler;
          [SerializeField] private RewardedAdHandler rewardedAdHandler;

          public bool AllowShowOpenAd => _allowShowOpenAd;
          private bool _allowShowOpenAd = true;

          private Action<bool> _appOpenAdCallback;
          private Action<bool> _interstitialAdCallback;
          private Action<bool> _rewardedAdCallback;

          public bool CanShowAds => !IsAdsRemoved()
                                 && InternetConnection.HasInternet()
                                 && OneHitConfigs.adsMode == AdsMode.PRODUCTION;


          #region =============== INITIALIZATION ===============

          private void OnValidate()
          {
               appOpenAdHandler = GetComponentInChildren<AppOpenAdHandler>();
               bannerAdHandler = GetComponentInChildren<BannerAdHandler>();
               interstitialAdHandler = GetComponentInChildren<InterstitialAdHandler>();
               rewardedAdHandler = GetComponentInChildren<RewardedAdHandler>();
          }

          public void Init()
          {
               Logger.Warning("<color=orange> </AdsManager> is initializing... </color>");
               InitAdMob();
               InitIronSource();
          }

          private void InitAdMob()
          {
               Logger.Warning("</AdsManager> start init AdMob...");

               MobileAds.Initialize(initStatus =>
               {
                    Logger.Warning("</AdsManager> AdMob init complete!!!");
                    LoadAppOpenAd();
               });
          }

          private void InitIronSource()
          {
               Logger.Warning("</AdsManager> start init IronSource...");

#if UNITY_ANDROID
               IronSource.Agent.init(OneHitConfigs.ironSourceAndroidAppKey);
#elif UNITY_IPHONE
               IronSource.Agent.init(OneHitConfigs.ironSourceIOSAppKey);
#endif
               IronSource.Agent.setConsent(true);
               IronSource.Agent.shouldTrackNetworkState(true);
               IronSource.Agent.validateIntegration();
               IronSource.Agent.setMetaData("do_not_sell", "false");
               IronSource.Agent.setMetaData("Facebook_IS_CacheFlag", "IMAGE");

               IronSourceEvents.onSdkInitializationCompletedEvent += () =>
               {
                    Logger.Warning("</AdsManager> IronSource init complete!!!");
                    LoadBanner();
                    LoadInterstitialAd();
                    LoadRewardedAd();
               };

               IronSourceEvents.onImpressionDataReadyEvent += (impressionData) =>
               {
                    SendRevenueToFirebase(impressionData);
                    SendRevenueToAdjust(impressionData);
               };
          }

          private void SendRevenueToFirebase(IronSourceImpressionData impressionData)
          {
               Logger.Warning("IronSource: ImpressionSuccessEvent impressionData = " + impressionData);
               if (impressionData == null) return;
               Parameter[] adParameters = {
                    new Parameter("ad_platform", "ironSource"),
                    new Parameter("ad_source", impressionData.adNetwork),
                    new Parameter("ad_unit_name", impressionData.adUnit),
                    new Parameter("ad_format", impressionData.instanceName),
                    new Parameter("currency", "USD"),
                    new Parameter("value", impressionData.revenue ?? 0)
               };
               FirebaseAnalytics.LogEvent("ad_impression", adParameters);
          }

          private void SendRevenueToAdjust(IronSourceImpressionData impressionData)
          {
               if (impressionData == null) return;
               double revenue = impressionData.revenue ?? 0;
               AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
               adjustAdRevenue.setRevenue(revenue, "USD");
               // optional fields
               adjustAdRevenue.setAdRevenueNetwork(impressionData.adNetwork);
               adjustAdRevenue.setAdRevenueUnit(impressionData.adUnit);
               adjustAdRevenue.setAdRevenuePlacement(impressionData.placement);
               // track Adjust ad revenue
               Adjust.trackAdRevenue(adjustAdRevenue);
          }

          private void OnApplicationPause(bool isPaused)
          {
               Logger.Warning($"</AdsManager> IronSource onApplicationPause: {isPaused}");
               IronSource.Agent.onApplicationPause(isPaused);
          }

          #endregion


          public void OnStartShowAd(bool isAppOpenAd = false)
          {
               /*----------------------------------------------------------------------------------------------------
               * IronSource doesn't have event when ad starts showing, so we need it to perform some actions:
               * - Mute sound, pause game
               * - Not allow show open ad
               *----------------------------------------------------------------------------------------------------*/

               Logger.Warning("</AdsManager> on open ad!!!");

               if (!isAppOpenAd)
               {
                    SoundManager.ChangePitch(0);
                    Time.timeScale = 0;
               }

               _allowShowOpenAd = false;
          }

          public void OnEndOfShowAd(bool isAppOpenAd = false)
          {
               Logger.Warning("</AdsManager> on close ad!!!");

               if (!isAppOpenAd)
               {
                    SoundManager.ChangePitch(1);
                    Time.timeScale = 1;
                    SetLastTimeShowAds();
               }

               _allowShowOpenAd = true;
          }


          #region =============== APP OPEN ===============

          public void LoadAppOpenAd()
          {
               Logger.Warning("</AdsManager> load app open ad...");

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't load ads!!!");
                    return;
               }

               appOpenAdHandler.Load();
          }

          public void ShowAppOpenAd(Action<bool> callback)
          {
               Logger.Warning("</AdsManager> show app open ad...");

               _appOpenAdCallback = callback;

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't show ads!!!");
                    OnAppOpenAdCallback(false);
                    return;
               }

               appOpenAdHandler.Show();
          }

          public void OnAppOpenAdCallback(bool showSuccess)
          {
               Logger.Warning($"</AdsManager> on app open ad callback: {showSuccess}");

               _appOpenAdCallback?.Invoke(showSuccess);
               _appOpenAdCallback = null;
          }

          #endregion


          #region =============== BANNER ===============

          public void LoadBanner()
          {
               Logger.Warning("</AdsManager> load banner...");

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't load ads!!!");
                    return;
               }

               bannerAdHandler.Load();
          }

          public void ShowBanner()
          {
               Logger.Info("</AdsManager> show banner...");

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't show ads!!!");
                    return;
               }

               bannerAdHandler.Show();
          }

          public void HideBanner()
          {
               Logger.Warning("</AdsManager> hide banner...");

               bannerAdHandler.Hide();
          }

          #endregion


          #region =============== INTERSTITIAL ===============

          public void LoadInterstitialAd()
          {
               Logger.Warning("</AdsManager> load interstitial ad...");

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't load ads!!!");
                    return;
               }

               interstitialAdHandler.Load();
          }

          public void ShowInterstitialAd(Action<bool> callback)
          {
               Logger.Warning("</AdsManager> show interstitial ad...");

               _interstitialAdCallback = callback;

#if UNITY_EDITOR
               Logger.Info("<color=lime> </AdsManager> skip interstitial ad in editor!!! </color>");
               OnInterstitialAdCallback(true);
               return;
#else
               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't show ads!!!");
                    OnInterstitialAdCallback(false);
                    return;
               }

               _interstitialAdHandler.Show();
#endif
          }

          public void OnInterstitialAdCallback(bool showSuccess)
          {
               Logger.Warning($"</AdsManager> on interstitial ad callback: {showSuccess}");

               _interstitialAdCallback?.Invoke(showSuccess);
               _interstitialAdCallback = null;
          }

          #endregion


          #region =============== REWARDED ===============

          public void LoadRewardedAd()
          {
               Logger.Warning("</AdsManager> load rewarded ad...");

               if (!CanShowAds)
               {
                    Logger.Error("</AdsManager> can't load ads!!!");
                    return;
               }

               rewardedAdHandler.Load();
          }

          public void ShowRewardedAd(Action<bool> callback)
          {
               Logger.Warning("</AdsManager> show rewarded ad...");

               _rewardedAdCallback = callback;

               //if (InternetConnection.Instance.isTestNoInternet)
               //{
               //     Logger.Error("AdsManager: is test no internet");
               //     OnRewardedVideoCallback(false);
               //     return;
               //}

#if UNITY_EDITOR
               Logger.Info("<color=lime> </AdsManager> skip rewarded ad in editor!!! </color>");
               OnRewardedAdCallback(true);
               return;
#else
               if (adsState == AdsState.UnlockAll)
               {
                    Logger.Warning("</AdsManager> skip rewarded ad (unlock all)");
                    OnRewardedAdCallback(true);
                    return;
               }

               _rewardedAdHandler.Show();
#endif
          }

          public void OnRewardedAdCallback(bool showSuccess)
          {
               Logger.Warning($"</AdsManager> on rewarded ad callback: {showSuccess}");

               if (!showSuccess)
               {
                    InternetConnection.Instance.ShowVideoNotReadyPanel();
               }
               //else Notification.Instance.Notify("No video available!\nPlease try again!");

               _rewardedAdCallback?.Invoke(showSuccess);
               _rewardedAdCallback = null;
          }

          #endregion


          /*-------------------- STATIC METHOD --------------------*/

          public static string GetLastTimeShowAds()
          {
               return PlayerPrefs.GetString("LastTimeShowAds", new DateTime(1990, 1, 1).ToString());
          }

          public static void SetLastTimeShowAds()
          {
               PlayerPrefs.SetString("LastTimeShowAds", DateTime.Now.ToString());
          }

          public static bool IsAdsRemoved()
          {
               return PlayerPrefs.GetInt("AdsRemoved", 1) == 1;
          }

          public static void SetAdsRemoved(bool remove = true)
          {
               PlayerPrefs.SetInt("AdsRemoved", remove ? 1 : 0);
          }
     }
}