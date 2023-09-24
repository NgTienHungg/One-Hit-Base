using UnityEngine;
using com.adjust.sdk;

namespace OneHit.SDK
{
     public class AdjustStarter : MonoBehaviour
     {
          public static void StartAdjust()
          {
               Adjust adjust = FindObjectOfType<Adjust>();
               AdjustConfig adjustConfig = new AdjustConfig(adjust.appToken, adjust.environment, (adjust.logLevel == AdjustLogLevel.Suppress));
               adjustConfig.setLogLevel(adjust.logLevel);
               adjustConfig.setSendInBackground(adjust.sendInBackground);
               adjustConfig.setEventBufferingEnabled(adjust.eventBuffering);
               adjustConfig.setLaunchDeferredDeeplink(adjust.launchDeferredDeeplink);
               adjustConfig.setDefaultTracker(adjust.defaultTracker);
               adjustConfig.setUrlStrategy(adjust.urlStrategy.ToLowerCaseString());
               adjustConfig.setAppSecret(adjust.secretId, adjust.info1, adjust.info2, adjust.info3, adjust.info4);
               adjustConfig.setDelayStart(adjust.startDelay);
               adjustConfig.setNeedsCost(adjust.needsCost);
               adjustConfig.setPreinstallTrackingEnabled(adjust.preinstallTracking);
               adjustConfig.setPreinstallFilePath(adjust.preinstallFilePath);
               adjustConfig.setAllowiAdInfoReading(adjust.iadInfoReading);
               adjustConfig.setAllowAdServicesInfoReading(adjust.adServicesInfoReading);
               adjustConfig.setAllowIdfaReading(adjust.idfaInfoReading);
               adjustConfig.setCoppaCompliantEnabled(adjust.coppaCompliant);
               adjustConfig.setPlayStoreKidsAppEnabled(adjust.playStoreKidsApp);
               adjustConfig.setLinkMeEnabled(adjust.linkMe);
               if (!adjust.skAdNetworkHandling)
               {
                    adjustConfig.deactivateSKAdNetworkHandling();
               }

               Adjust.start(adjustConfig);
               AdjustThirdPartySharing adjustThirdPartySharing = new AdjustThirdPartySharing(true);
               Adjust.trackThirdPartySharing(adjustThirdPartySharing);
          }
     }
}