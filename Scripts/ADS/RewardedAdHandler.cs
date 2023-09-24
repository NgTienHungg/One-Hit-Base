using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace OneHit.ADS
{
     public class RewardedAdHandler : MonoBehaviour, IAdHandler
     {
          private float _loadAdWaitTime;

          public void Init()
          {
               Logger.Warning("</Rewarded> is initializing...");
               RegisterRewardedVideoEvents();

               _loadAdWaitTime = OneHitConfigs.rewardedAdLoadWaitTime;
          }

          public void Load()
          {
               Logger.Warning("</Rewarded> is loading...");
               IronSource.Agent.loadRewardedVideo();
          }

          public async void Show()
          {
               // reload ad and wait a short time if ad not ready
               if (!IronSource.Agent.isRewardedVideoAvailable())
               {
                    Logger.Error("</Rewarded> not available, reload...");
                    AdsManager.Instance.LoadRewardedAd();

                    Logger.Warning($"</Rewarded> wait until available... (no more {_loadAdWaitTime}s)");
                    var timeOut = DateTime.Now.AddSeconds(_loadAdWaitTime);
                    await UniTask.WaitUntil(() => IronSource.Agent.isRewardedVideoAvailable() || DateTime.Now > timeOut);
               }

               // show ad if ready
               if (IronSource.Agent.isRewardedVideoAvailable())
               {
                    Logger.Warning("</Rewarded> ready to show!");
                    IronSource.Agent.showRewardedVideo();
                    AdsManager.Instance.OnStartShowAd();
               }
               else
               {
                    Logger.Error("</Rewarded> not ready to show!");
                    AdsManager.Instance.OnRewardedAdCallback(false);
               }
          }

          public void Hide() { }


          #region =====> IRONSOURCE REWARDED VIDEO EVENTS <=====

          private void RegisterRewardedVideoEvents()
          {
               IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
               IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
               IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
               IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
               IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
               IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
               IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
          }

          // Indicates that there’s an available ad.
          // The adInfo object includes information about the ad that was loaded successfully
          private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Rewarded Video On Ad Available");
          }

          // Indicates that no ads are available to be displayed
          private void RewardedVideoOnAdUnavailable()
          {
               Logger.Warning("</IronSource> Rewarded Video On Ad Unavailable");
          }

          // The Rewarded Video ad view has opened. Your activity will loose focus.
          private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Rewarded Video On Ad Opened Event");
          }

          // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
          private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Rewarded Video On Ad Closed Event");
               AdsManager.Instance.LoadRewardedAd();
               AdsManager.Instance.OnEndOfShowAd();
          }

          // The user completed to watch the video, and should be rewarded.
          // The placement parameter will include the reward data.
          // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
          private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
          {
               Logger.Warning($"</IronSource> Rewarded Video On Ad Rewarded Event (name = {placement.getRewardName()}, amount = {placement.getRewardAmount()})");
               FirebaseManager.LogEvent(FirebaseEvent.AD_REWARD);
               AdsManager.Instance.OnRewardedAdCallback(true);
          }

          // The rewarded video ad was failed to show.
          private void RewardedVideoOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
          {
               Logger.Error($"</IronSource> Rewarded Video On Ad Show Failed Event (code: {ironSourceError.getCode()}, description: {ironSourceError.getDescription()})");
               AdsManager.Instance.OnRewardedAdCallback(false);
          }

          // Invoked when the video ad was clicked.
          // This callback is not supported by all networks, and we recommend using it only if
          // it’s supported by all networks you included in your build.
          private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
          {
               Logger.Warning($"</IronSource> Rewarded Video On Ad Rewarded Event (name = {placement.getRewardName()}, amount = {placement.getRewardAmount()})");
          }

          #endregion
     }
}