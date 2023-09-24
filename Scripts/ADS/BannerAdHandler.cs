using UnityEngine;
using Cysharp.Threading.Tasks;

namespace OneHit.ADS
{
     public class BannerAdHandler : MonoBehaviour, IAdHandler
     {
          private bool _allowShow;
          private bool _readyToShow;

          public void Init()
          {
               Logger.Warning("</Banner> is initializing...");
               RegisterBannerEvents();

               _allowShow = false;
               _readyToShow = false;
          }

          public void Load()
          {
               Logger.Warning("</Banner> is loading...");
               IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
          }

          public async void Show()
          {
               _allowShow = true;

               if (!_readyToShow)
               {
                    Logger.Error("</Banner> not ready, reload...");
                    AdsManager.Instance.LoadBanner();

                    Logger.Warning("</Banner> wait until ready...");
                    await UniTask.WaitUntil(() => _readyToShow);
               }

               // await banner ready to show but banner was hidden
               if (!_allowShow)
               {
                    Logger.Warning("</Banner> is ready but not allow show!");
                    IronSource.Agent.hideBanner();
                    return;
               }

               IronSource.Agent.displayBanner();
          }

          public void Hide()
          {
               Logger.Warning("</Banner> is hiding...");
               IronSource.Agent.hideBanner();
               _allowShow = false;
          }


          #region =====> IRON SOURCE BANNER EVENTS <=====

          private void RegisterBannerEvents()
          {
               IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
               IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
               IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
               IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
               IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
               IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;
          }

          // Invoked once the banner has loaded
          private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Banner On Ad Loaded Event");
               _readyToShow = true;

               if (!_allowShow)
               {
                    Logger.Warning("</Banner> not allow show!");
                    IronSource.Agent.hideBanner();
               }
          }

          // Invoked when the banner loading process has failed.
          private void BannerOnAdLoadFailedEvent(IronSourceError error)
          {
               Logger.Error("</IronSource> Banner On Ad Load Failed Event: " + error.getCode() + ", description : " + error.getDescription());
               AdsManager.Instance.LoadBanner();
               _readyToShow = false;
          }

          // Invoked when end user clicks on the banner ad
          private void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Banner On Ad Clicked Event");
          }

          // Notifies the presentation of a full screen content following user click
          private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Banner On Ad Screen Presented Event");
          }

          // Notifies the presented screen has been dismissed
          private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Banner On Ad Screen Dismissed Event");
          }

          // Invoked when the user leaves the app
          private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
          {
               Logger.Warning("</IronSource> Banner On Ad Left Application Event");
          }

          #endregion
     }
}

/*----------------------------------------------------------------------------------------------------
 *
 * 1. BANNER shows up automatically when loaded, so we need 'readyToShowBanner' flag to check if BANNER
 *    has loaded or failed to load.
 *    
 * 2. While "wait until BANNER ready", BANNER can be hiden by 'Hide()' function,so we need 'allowShowBanner'
 *    flag to check when BANNER is loaded, we hide it.
 *    
 * 3. Some special case:
 *    + in Loading scene => Banner loaded => need to banner.Hide()
 *    + call ShowBanner() => wait banner loaded => then call HideBanner() => banner loaded => need to banner.Hide()
 *    + No Internet Connection => stop load banner => Has Internet Connection => need to call ShowBanner()
 *    
 *----------------------------------------------------------------------------------------------------*/