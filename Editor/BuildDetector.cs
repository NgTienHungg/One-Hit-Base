using UnityEngine;
using UnityEditor.Build.Reporting;

namespace OneHit.Editor
{
     public class BuildDetector : UnityEditor.Build.IPostprocessBuildWithReport
     {
          public int callbackOrder => 0;

          public void OnPostprocessBuild(BuildReport report)
          {
               Debug.Log("Build result: " + report.summary.result);

               if (report.summary.result == BuildResult.Succeeded)
               {
                    string path = "python -u \"c:\\Users\\84946\\OneDrive - ptit.edu.vn\\Desktop\\UpFileGgDrive.py\"";
                    PythonRunner.RunCommand(path);
               }
               else
               {
                    string path = "python -u \"c:\\Users\\84946\\OneDrive - ptit.edu.vn\\Desktop\\UpFileGgDrive.py\"";
                    PythonRunner.RunCommand(path);
               }
          }
     }
}