using UnityEngine;

namespace OneHit
{
     public static class Logger
     {
          public static void Info(string message, GameObject context = default)
          {
               if (OneHitConfigs.enableLogger)
               {
                    Debug.Log(message, context);
               }
          }

          public static void Warning(string message, GameObject context = default)
          {
               if (OneHitConfigs.enableLogger)
               {
                    Debug.LogWarning(message, context);
               }
          }

          public static void Error(string message, GameObject context = default)
          {
               if (OneHitConfigs.enableLogger)
               {
                    Debug.LogError(message, context);
               }
          }
     }
}