using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace OneHit.Internet
{
     public class InternetConnection : Singleton<InternetConnection>
     {
          [Header("Panels")]
          [SerializeField] private DarkBGPanel noInternetPanel;
          [SerializeField] private DarkBGPanel noVideoPanel;

          [Space, Header("Configuration")]
          [InfoBox("<color=red>Internet required</color> to play the game?")]
          [SerializeField] private bool requireInternet;

          [ShowIf("requireInternet", true)]
          [SerializeField] private float timePerInternetCheck = 1f;

          [Space, Header("Test")]
          [InfoBox("True: always show NoVideoPanel when show Reward Ads")]
          public bool isTestNoInternet;


          private void Start()
          {
               LogInternetStatus();
               StartCoroutine(WaitCheckInternet());
          }


          private void LogInternetStatus()
          {
               if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                    Debug.LogWarning("<color=lime> Internet: Network is available through wifi! </color>");

               if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                    Debug.LogWarning("<color=lime> Internet: Network is available through mobile data! </color>");

               if (Application.internetReachability == NetworkReachability.NotReachable)
                    Debug.LogError("<color=red> Internet: Network not available! </color>");
          }


          private IEnumerator WaitCheckInternet()
          {
               // don't check internet if not require
               if (!requireInternet) yield break;

               // wait scene LOADING complete, check internet in scene MAIN
               yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex != 0);

               Debug.LogWarning("<color=cyan> [Internet]: Start check internet... </color>");
               var waitForSeconds = new WaitForSeconds(timePerInternetCheck);

               while (true)
               {
                    yield return waitForSeconds;

                    if (!HasInternet())
                    {
                         noInternetPanel.Enable();
                         yield return new WaitUntil(HasInternet);
                         noInternetPanel.Disable();
                    }
               }
          }


          public void OpenWifiSetting()
          {
#if UNITY_ANDROID
               try
               {
                    AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                    intent.Call<AndroidJavaObject>("setAction", "android.settings.WIFI_SETTINGS");

                    AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                    currentActivity.Call("startActivity", intent);
               }
               catch (Exception e)
               {
                    Debug.LogError(e);
               }
#endif
          }


          public void ShowVideoNotReadyPanel()
          {
               noVideoPanel.Enable();
          }


          public static bool HasInternet()
          {
               return Application.internetReachability != NetworkReachability.NotReachable;
          }
     }
}