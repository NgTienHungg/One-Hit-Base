using System.IO;
using UnityEditor;
using UnityEngine;

namespace OneHit.Editor
{
     public class OneHitSettingsTool
     {
          private const string SETTINGS_RESOURCE_DIR = "Assets/~OneHit/Resources";
          private const string ONEHIT_SETTINGS_FILE = "OneHitSettings";
          private const string SETTINGS_FILE_EXTENSION = ".asset";

          [MenuItem("OneHit/Settings")]
          public static void OpenSettings()
          {
               // Read from resources.
               var settings = Resources.Load<OneHitSettings>(ONEHIT_SETTINGS_FILE);

               // Create instance if null.
               if (settings == null)
               {
                    Directory.CreateDirectory(SETTINGS_RESOURCE_DIR);
                    settings = ScriptableObject.CreateInstance<OneHitSettings>();
                    string assetPath = Path.Combine(SETTINGS_RESOURCE_DIR, ONEHIT_SETTINGS_FILE + SETTINGS_FILE_EXTENSION);
                    AssetDatabase.CreateAsset(settings, assetPath);
                    AssetDatabase.SaveAssets();
               }

               // Select instance
               Selection.activeObject = settings;
          }
     }
}