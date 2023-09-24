using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OneHit.ADS;

namespace OneHit
{

     public class MasterControl : MonoBehaviour
     {

          public static MasterControl Instance { get; private set; }

          [Header("Components")]
          public AdsManager _adsManager;

          private void Awake()
          {
               #region Singleton
               if (Instance != null)
               {
                    Destroy(gameObject);
                    return;
               }
               Instance = this;
               DontDestroyOnLoad(gameObject);
               #endregion
          }

          private async void Start()
          {
               _adsManager.Init();
               await UniTask.Delay(500);
          }

          #region =============== ADS ===============

          public void ShowBanner()
          {
               _adsManager.ShowBanner();
          }

          public void HideBanner()
          {
               _adsManager.HideBanner();
          }

          public void ShowInterAd(Action<bool> callback = null)
          {
               _adsManager.ShowInterstitialAd(callback);
          }

          public void ShowRewardAd(Action<bool> callback)
          {
               _adsManager.ShowRewardedAd(callback);
          }

          public void ShowOpenAd(Action<bool> callback = null)
          {
               _adsManager.ShowAppOpenAd(callback);
          }

          #endregion
     }
}