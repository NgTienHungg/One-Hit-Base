using System;
using Firebase;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

namespace OneHit
{
     public class FirebaseManager : MonoBehaviour
     {
          private static FirebaseManager _instance;

          public static FirebaseManager Instance
          {
               get
               {
                    if (_instance == null)
                    {
                         _instance = FindObjectOfType<FirebaseManager>();
                         DontDestroyOnLoad(_instance.gameObject);
                    }
                    return _instance;
               }
          }

          [ReadOnly] public bool isInitialized;

          private void Start() => Initialize();

          private void Initialize()
          {
               Debug.Log("<color=orange> [Firebase]: start init... </color>");

               FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
               {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                         Debug.Log("<color=lime> [Firebase]: initialized!! </color>");
                         isInitialized = true;

                         FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                         FetchDataAsync();
                    }
                    else
                    {
                         Debug.LogError("[Firebase]: Could not resolve all Firebase dependencies: " + dependencyStatus);
                    }
               });
          }

          private async void FetchDataAsync()
          {
               Debug.Log("<color=cyan> [Firebase]: fetching data... </color>");
               await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero)
                    .ContinueWithOnMainThread(FetchComplete);
          }

          private void FetchComplete(Task fetchTask)
          {
               if (!fetchTask.IsCompleted)
               {
                    Debug.LogError("[Firebase]: Retrieval hasn't finished.");
                    return;
               }

               var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
               var info = remoteConfig.Info;
               if (info.LastFetchStatus != LastFetchStatus.Success)
               {
                    Debug.LogError($"[Firebase]: Fetch data was unsuccessful: {info.LastFetchStatus}");
                    return;
               }

               // Fetch successful. Parameter values must be activated to use.
               remoteConfig.ActivateAsync().ContinueWithOnMainThread(task =>
               {
                    Debug.Log($"<color=lime> [Firebase]: Remote data loaded and ready for use. Last fetch time {info.FetchTime} </color>");

                    #region ===== Data in firebase =====
                    ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_time_iap");
                    Debug.Log("AdSetting_time_iap: " + value.StringValue);
                    if (!string.IsNullOrEmpty(value.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_time_iap", value.StringValue);
                    }

                    ConfigValue value1 = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_time_reward");
                    Debug.Log("AdSetting_time_reward: " + value1.StringValue);
                    if (!string.IsNullOrEmpty(value1.StringValue))
                    {
                         PlayerPrefs.SetInt("AdSetting_time_reward", int.Parse(value1.StringValue));
                    }

                    ConfigValue value2 = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_time_normal");
                    Debug.Log("AdSetting_time_normal: " + value2.StringValue);
                    if (!string.IsNullOrEmpty(value2.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_time_normal", value2.StringValue);
                    }

                    ConfigValue value3 = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_play");
                    Debug.Log("AdSetting_play: " + value3.StringValue);
                    if (!string.IsNullOrEmpty(value3.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_play", value3.StringValue);
                    }

                    ConfigValue value4 = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_level");
                    Debug.Log("AdSetting_level: " + value4.StringValue);
                    if (!string.IsNullOrEmpty(value4.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_level", value4.StringValue);
                    }

                    ConfigValue value5 = FirebaseRemoteConfig.DefaultInstance.GetValue("level_show_rate");
                    Debug.Log("level_show_rate: " + value5.StringValue);
                    if (!string.IsNullOrEmpty(value5.StringValue))
                    {
                         PlayerPrefs.SetInt("level_show_rate", int.Parse(value5.StringValue));
                    }

                    ConfigValue ads_play = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_play_gateplay");
                    Debug.Log("AdSetting_play_gateplay: " + ads_play.StringValue);
                    if (!string.IsNullOrEmpty(ads_play.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_play_gateplay", ads_play.StringValue);
                    }

                    ConfigValue ads_time = FirebaseRemoteConfig.DefaultInstance.GetValue("AdSetting_time_gateplay");
                    Debug.Log("AdSetting_time_gateplay: " + ads_time.StringValue);
                    if (!string.IsNullOrEmpty(ads_time.StringValue))
                    {
                         PlayerPrefs.SetString("AdSetting_time_gateplay", ads_time.StringValue);
                    }
                    #endregion
               });
          }

          public static void LogEvent(string eventName)
          {
               if (!Instance.isInitialized)
               {
                    Debug.LogError("[Firebase]: not initialized");
                    return;
               }

               try
               {
                    Debug.Log($"<color=cyan> [Firebase] </color>: <color=yellow> {eventName} </color>");
                    FirebaseAnalytics.LogEvent(eventName);
               }
               catch (FirebaseException e)
               {
                    Debug.LogError(e);
               }
          }
     }
}