using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FifaFootballGame.Service
{
    public class PythonExeRunner
    {
        public void Start()
        {
            try
            {
                string command = "python3 -m uvicorn FirstApp:app"; // или uvicorn FirstApp:app --reload

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = psi };

                process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine("ERR: " + e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                MessageBox.Show("Скрипт запущен через PowerShell. Нажмите Enter для выхода...");
                

                if (!process.HasExited)
                    process.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        
        }
    }
}
