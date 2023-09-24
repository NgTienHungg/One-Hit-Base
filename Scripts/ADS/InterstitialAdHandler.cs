using System;
using UnityEngine;

namespace OneHit.ADS
{
     public class InterstitialAdHandler : MonoBehaviour, IAdHandler
     {
          private float _timeDifferenceAllow;

          public void Init()
          {
               Logger.Warning("</Interstitial> is initializing...");
               RegisterInterstitialEvents();

               _timeDifferenceAllow = OneHitConfigs.timeAllowedShowInterstitial;
          }

          public void Load()
          {
               Logger.Warning("</Interstitial> is loading...");
               IronSource.Agent.loadInterstitial();
          }

          public void Show()
          {
               // not allow showing 2 ads in a short time
               if (!IsAllowShowInterstital())
               {
                    Logger.Warning("</Interstitial> not allow to show!");
                    AdsManager.Instance.OnInterstitialAdCallback(false);
                    return;
               }

               // reload ad and skip this turn if ad not ready
               if (!IronSource.Agent.isInterstitialReady())
               {
                    Logger.Error("</Interstitial> not ready, reload...");
                    IronSource.Agent.loadInterstitial();
                    return;
               }

               // show ad if ready
               if (IronSource.Agent.isInterstitialReady())
               {
                    Logger.Warning("</Interstitial> ready to show!");
                    IronSource.Agent.showInterstitial();
                    AdsManager.Instance.OnStartShowAd();
               }
               else
               {
                    Logger.Error("</Interstitial> not ready to show!");
                    AdsManager.Instance.OnInterstitialAdCallback(false);
               }
          }

          public void Hide() { }

          public bool IsAllowShowInterstital()
          {
               var lastTimeShowAd = DateTime.Parse(AdsManager.GetLastTimeShowAds());
               double time = (DateTime.Now - lastTimeShowAd).TotalSeconds;
               return time >= _timeDifferenceAllow;
          }


          #region =====> IRONSOURCE INTERSTITIAL EVENTS <=====

          private void RegisterInterstitialEvents()
          {
               IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
               IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
               IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
               IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
               IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
               IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
               IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
          }

          // Invoked when the interstitial ad was loaded succesfully.
          private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Interstitial On Ad Ready Event");
          }

          // Invoked when the initialization process has failed.
          private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
          {
               Logger.Error($"</IronSource> Interstitial On Ad Load Failed (code: {ironSourceError.getCode()}, description: {ironSourceError.getDescription()})");
          }

          // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
          private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Interstitial On Ad Opened Event");
               //? This event is called at the same time as 'InterstitialOnAdClosedEvent'
          }

          // Invoked when the interstitial ad closed and the user went back to the application screen.
          private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Interstitial On Ad Closed Event");
               AdsManager.Instance.LoadInterstitialAd();
               AdsManager.Instance.OnEndOfShowAd();
          }

          // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
          // This callback is not supported by all networks, and we recommend using it only if it's supported by all networks you included in your build.
          private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Interstitial On Ad Show Succeeded Event");
               FirebaseManager.LogEvent(FirebaseEvent.AD_INTERSTITIAL);
               AdsManager.Instance.OnInterstitialAdCallback(true);
          }

          // Invoked when the ad failed to show.
          private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
          {
               Logger.Error($"</IronSource> Interstitial On Ad Show Failed Event (code: {ironSourceError.getCode()}, description: {ironSourceError.getDescription()})");
               AdsManager.Instance.OnInterstitialAdCallback(false);
          }

          // Invoked when end user clicked on the interstitial ad
          private void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Interstitial On Ad Clicked Event");
          }

          #endregion
     }
}

/*----------------------------------------------------------------------------------------------------
 *
 * 1. There should be a time limit between two consecutive interstitial impressions: _timeDifferenceAllow
 *    
 * 2. Don't reload ad from Event "InterstitialOnAdLoadFailed"
 * 
 *----------------------------------------------------------------------------------------------------*/