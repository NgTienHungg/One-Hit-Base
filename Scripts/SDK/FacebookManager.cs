using UnityEngine;
using Facebook.Unity;
using System.Collections.Generic;

namespace OneHit
{
     public class FacebookManager : MonoBehaviour
     {
          public static FacebookManager Instance { get; private set; }

          private void Awake()
          {
               if (FB.IsInitialized)
               {
                    Debug.Log("<color=lime> INIT FACEBOOK </color>");
                    FB.ActivateApp();
               }
               else
               {
                    Debug.Log("<color=red> NOT INIT FACEBOOK </color>");
                    FB.Init(() =>
                    {
                         FB.ActivateApp();
                         //FB.Mobile.FetchDeferredAppLinkData(AppLinkCallback);
                    });
               }
          }

          public void Login()
          {
               var perms = new List<string>() { "public_profile", "email" };
               FB.LogInWithReadPermissions(perms, AuthCallback);
          }

          public void Login(FacebookDelegate<ILoginResult> callback)
          {
               var perms = new List<string>() { "public_profile", "email" };
               FB.LogInWithReadPermissions(perms, callback);
          }

          private void AuthCallback(ILoginResult result)
          {
               if (FB.IsLoggedIn)
               {
                    // todo
               }
               else
                    Debug.Log("User cancelled login");
          }

          public void FacebookGameRequest()
          {
               if (FB.IsLoggedIn)
                    FB.AppRequest("This game is real!!", title: "Hero Rescue", callback: AppRequestCallback);
               else
                    Login();
          }

          public void ShareLink()
          {
               if (FB.IsLoggedIn)
                    FB.ShareLink(new System.Uri("http://tiny.cc/HeroRescue"), "Hero Rescue", "OMG This game is real now! Download now: http://tiny.cc/HeroRescue", callback: ShareCallback);
               else
                    Login();
          }

          private void AppRequestCallback(IAppRequestResult result)
          {
               if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
                    Debug.Log("request Error: " + result.Error);
               else
                    Debug.Log("request success!");
          }

          private void ShareCallback(IShareResult result)
          {
               if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
                    Debug.Log("ShareLink Error: " + result.Error);
               else
                    Debug.Log("ShareLink success!");
          }

          //public void Log(string eventName) {
          //    try {
          //        Debug.Log("FB LOG: " + eventName);
          //        FB.LogAppEvent(eventName);
          //    }
          //    catch { }
          //}
     }
}