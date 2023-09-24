using OneHit.ADS;
using UnityEngine;

public class LargeNativeAds : NativeAdHandler
{

     private int playCount;

     private void OnEnable()
     {
          TryShow();
     }

     public override void TryShow()
     {
          if (AdsManager.IsAdsRemoved()) return;
          if (adReadyToShow)
          {
               gameObject.SetActive(true);
          }
          RequestNativeAdHandle();

          playCount = 0;
     }

     private void OnPlay()
     {
          playCount++;
          if (playCount % 2 == 0)
               RequestNativeAdHandle();
     }

     public override void AdLoadedHandle()
     {
          Debug.LogWarning("Native ad: Large ad loaded");
          gameObject.SetActive(true);
     }

     public override void AdFailedLoadHandle()
     {
          Debug.LogError("Native ad: Large ad load failed");
     }
}