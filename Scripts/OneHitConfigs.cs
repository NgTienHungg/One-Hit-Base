using OneHit.ADS;

namespace OneHit
{
     /* These fields are set when changing the value on OneHitSettings */
     public static class OneHitConfigs
     {
          public static AdsMode adsMode;
          public static bool enableLogger;

          public static float appOpenAdLoadWaitTime;
          public static float rewardedAdLoadWaitTime;
          public static float timeAllowedShowInterstitial;

          /*--------------------------------------------------*/

          // IronSource App Key
          public static string ironSourceAndroidAppKey;
          public static string ironSourceIOSAppKey;

          // Admob App Id
          public static string admobAndroidAppId;
          public static string admobIOSAppId;

          // Admob App Open Ad
          public static string androidAppOpenAdId;
          public static string iOSAppOpenAdId;

          // Admob App Open Ad Test
          public static string androidAppOpenAdTestId;
          public static string iOSAppOpenAdTestId;

          // Admob Native Ad
          public static string androidNativeAdId;
          public static string iOSNativeAdId;

          // Admob Native Ad Test
          public static string androidNativeAdTestId;
          public static string iOSNativeAdTestId;
     }
}