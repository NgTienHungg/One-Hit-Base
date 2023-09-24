using OneHit.ADS;
using UnityEngine;
using UnityEditor;
using com.adjust.sdk;
using Sirenix.OdinInspector;

namespace OneHit.Editor
{
     public class OneHitSettings : ScriptableObject
     {
          [Space(30), Title("Ads Config")]
          [EnumToggleButtons] public AdsMode adsMode;
          [Space] public bool enableLogger;

          [Space]
          [Range(0.5f, 3f)] public float appOpenAdLoadWaitTime = 2f;
          [Range(0.5f, 3f)] public float rewardedAdLoadWaitTime = 2f;
          [Range(15f, 60f)] public float timeAllowedShowInterstitial = 15f;

          [Title("GG Drive")]
          [InlineButton(nameof(OpenApkDrive), "Open")] public string apkDriveLink;
          [InlineButton(nameof(OpenAabDrive), "Open")] public string aabDriveLink;

          /*--------------------------------------------------*/

          [Space(30), Title("IronSource App Key")]
          [GUIColor(0, 0.8f, 1f)] public string ironSourceAndroidAppKey;
          [GUIColor(0, 0.8f, 1f)] public string ironSourceIOSAppKey;

          [Title("AdMob App ID")]
          [GUIColor(0, 0.8f, 1f)] public string admobAndroidAppId;
          [GUIColor(0, 0.8f, 1f)] public string admobIOSAppId;

          [Title("App Open Ad")]
          [GUIColor(0, 0.8f, 1f)] public string androidAppOpenAdId;
          [GUIColor(0, 0.8f, 1f)] public string iOSAppOpenAdId;

          [ReadOnly] public string androidAppOpenAdTestId = "ca-app-pub-3940256099942544/3419835294";
          [ReadOnly] public string iOSAppOpenAdTestId = "ca-app-pub-3940256099942544/5662855259";

          [Title("Native Ad")]
          [GUIColor(0, 0.8f, 1f)] public string androidNativeAdId;
          [GUIColor(0, 0.8f, 1f)] public string iOSNativeAdId;

          [ReadOnly] public string androidNativeAdTestId = "ca-app-pub-3940256099942544/2247696110";
          [ReadOnly] public string iOSNativeAdTestId = "ca-app-pub-3940256099942544/3986624511";

          [Title("Adjust")]
          [GUIColor(0, 0.8f, 1f)] public string adjustAndroidAppToken;
          [GUIColor(0, 0.8f, 1f)] public string adjustIOSAppToken;

          /*--------------------------------------------------*/

          #region UTILITY BUTTONS
          [PropertySpace(30)]
          [Title("Utility Buttons")]
          [InfoBox("Một số SDK đã được tự động setup key, ID ads, có thể check tại đây!")]
          [Button("Open IronSource Mediation Settings")]
          public void OpenIronSourceMediationSettings()
          {
               EditorApplication.ExecuteMenuItem("Ads Mediation/Developer Settings/LevelPlay Mediation Settings");
          }

          [Button("Open IronSouce Mediated Network Setttings")]
          public void OpenIronSourceMediatedNetworkSettings()
          {
               EditorApplication.ExecuteMenuItem("Ads Mediation/Developer Settings/Mediated Network Settings");
          }

          [PropertySpace]
          [InfoBox("Một số SDK không hỗ trợ truy cập vào thuộc tính nên không thể tự động setup, hãy làm thủ công!")]
          [Button("Open Google Mobile Ads Settings")]
          public void OpenGoogleMobileAdsSettings()
          {
               EditorApplication.ExecuteMenuItem("Assets/Google Mobile Ads/Settings...");
          }

          [Button("Open Facebook Settings")]
          public void OpenFaceBookSettings()
          {
               EditorApplication.ExecuteMenuItem("Facebook/Edit Settings");
          }

          [PropertySpace]
          [InfoBox("Cập nhật / Làm mới các dependency để chúng tương thích với nhau!")]
          [Button("Force Resolve External Dependency")]
          public void ForceResolveExternalDependencies()
          {
               EditorApplication.ExecuteMenuItem("Assets/External Dependency Manager/Android Resolver/Force Resolve");
          }

          public void OpenApkDrive()
          {
               Application.OpenURL(apkDriveLink);
          }

          public void OpenAabDrive()
          {
               Application.OpenURL(aabDriveLink);
          }
          #endregion

          #region HANDLE INPUT
          private void OnValidate()
          {
               UpdateIronSourceData();
               UpdateAdmobData();
               UpdateAdjustSDK();
               UpdateConfig();
          }

          private void UpdateIronSourceData()
          {
               var ISMS = Resources.Load<IronSourceMediationSettings>("IronSourceMediationSettings");
               ISMS.AndroidAppKey = ironSourceAndroidAppKey.Trim();
               ISMS.IOSAppKey = ironSourceIOSAppKey.Trim();
               ISMS.EnableIronsourceSDKInitAPI = true;
               ISMS.AddIronsourceSkadnetworkID = true;
               ISMS.DeclareAD_IDPermission = true;

               //? NOTE: need put this script in Editor folder
               var ISMNS = Resources.Load<IronSourceMediatedNetworkSettings>("IronSourceMediatedNetworkSettings");
               ISMNS.EnableAdmob = true;
               ISMNS.AdmobAndroidAppId = admobAndroidAppId.Trim();
               ISMNS.AdmobIOSAppId = admobIOSAppId.Trim();
          }

          private void UpdateAdmobData()
          {
               //? NOTE: need change 'internal' -> 'public' of class GoogleMobileAdsSettings
               //var GBAS = Resources.Load<GoogleMobileAdsSettings>("GoogleMobileAdsSettings");
               //GBAS.GoogleMobileAdsAndroidAppId = admobAndroidAppId;
               //GBAS.GoogleMobileAdsIOSAppId = admobIOSAppId;
               //GBAS.OptimizeInitialization = true;
               //GBAS.OptimizeAdLoading = true;
               //GBAS.DelayAppMeasurementInit = true;
          }

          private void UpdateAdjustSDK()
          {
               var adjust = FindObjectOfType<Adjust>();
#if UNITY_ANDROID
               adjust.appToken = adjustAndroidAppToken;
#elif UNITY_IOS
               adjust.appToken = adjustIOSAppToken;
#endif
          }

          private void UpdateConfig()
          {
               // Ads Config
               OneHitConfigs.adsMode = adsMode;
               OneHitConfigs.enableLogger = enableLogger;

               OneHitConfigs.appOpenAdLoadWaitTime = appOpenAdLoadWaitTime;
               OneHitConfigs.rewardedAdLoadWaitTime = rewardedAdLoadWaitTime;
               OneHitConfigs.timeAllowedShowInterstitial = timeAllowedShowInterstitial;

               // IronSource App Key
               OneHitConfigs.ironSourceAndroidAppKey = ironSourceAndroidAppKey;
               OneHitConfigs.ironSourceIOSAppKey = ironSourceIOSAppKey;

               // Admob App Open Ad
               OneHitConfigs.androidAppOpenAdId = androidAppOpenAdId;
               OneHitConfigs.iOSAppOpenAdId = iOSAppOpenAdId;

               // Admob App Open Ad Test
               OneHitConfigs.androidAppOpenAdTestId = androidAppOpenAdTestId;
               OneHitConfigs.iOSAppOpenAdTestId = iOSAppOpenAdTestId;

               // Admob Native Ad
               OneHitConfigs.androidNativeAdId = androidNativeAdId;
               OneHitConfigs.iOSNativeAdId = iOSNativeAdId;

               // Admob Native Ad Test
               OneHitConfigs.androidNativeAdTestId = androidNativeAdTestId;
               OneHitConfigs.iOSNativeAdTestId = iOSNativeAdTestId;
          }
          #endregion
     }
}

/* Lắng nghe sự thay đổi các giá trị trên Inspector bằng OnValidate()
 * Cập nhật các giá trị đó tới các Config của SDK IronSource, Admob
 * Cập nhật các giá trị tới OneHitConfig
 */