using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OneHit.Notification
{
     [Serializable]
     public class Notification
     {
          public string title;
          public string message;

          public Notification(string title, string message)
          {
               this.title = title;
               this.message = message;
          }

          public string GetMessageWithIcon()
          {
               try
               {
                    string version = SystemInfo.operatingSystem;
                    string[] iconTexts = { " " };
                    if (!version.Contains("4.0") && !version.Contains("4.1") && !version.Contains("4.2") && !version.Contains("4.3") && !version.Contains("4.4") && !version.Contains("5.0") && !version.Contains("5.1") && !version.Contains("5.2"))
                    {
                         iconTexts = new string[] { "🎁", "🤩", "⏰" };
                    }

                    return iconTexts[Random.Range(0, iconTexts.Length)] + message + iconTexts[Random.Range(0, iconTexts.Length)];
               }
               catch (Exception e)
               {
                    Debug.LogError(e);
                    return message;
               }
          }
     }
}