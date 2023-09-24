using System;
using System.Diagnostics;

namespace OneHit.Editor
{
     public class PythonRunner
     {
          public static void RunPythonFile(string filePath)
          {
               // Đường dẫn đến trình thông dịch Python
               string pythonPath = "C:\\Users\\84946\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";

               // Tạo một quá trình để thực thi tệp Python
               ProcessStartInfo startInfo = new ProcessStartInfo
               {
                    FileName = pythonPath,
                    Arguments = filePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
               };

               // Bắt đầu quá trình thực thi Python
               Process process = new Process();
               process.StartInfo = startInfo;
               process.Start();

               // Đọc kết quả đầu ra từ quá trình
               string output = process.StandardOutput.ReadToEnd();

               // Chờ quá trình kết thúc
               process.WaitForExit();

               // In kết quả đầu ra
               UnityEngine.Debug.Log(output);
          }

          public static void RunCommand(string command)
          {
               // Tạo một quá trình để chạy lệnh cmd
               Process process = new Process();
               process.StartInfo.FileName = "cmd.exe";
               process.StartInfo.UseShellExecute = false;
               process.StartInfo.RedirectStandardOutput = true;
               process.StartInfo.CreateNoWindow = true;

               // Chạy lệnh cmd
               process.StartInfo.Arguments = "/c " + command;
               process.Start();

               // Đọc kết quả đầu ra từ quá trình
               string output = process.StandardOutput.ReadToEnd();

               // Chờ quá trình kết thúc
               process.WaitForExit();

               // In kết quả đầu ra
               Console.WriteLine(output);
          }
     }
}