using System;
using System.Diagnostics;
using System.IO;

namespace SignalStrengthReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var timeNow = GetCurrentTime();

            DisplayMessage("Press any key to stop.\n\nDate Time" + "\t\t" + "Signal Strength %" + "\t\t" + "dBm");

            while (!Console.KeyAvailable)
            {
                if (timeNow.Hour == GetCurrentTime().Hour && timeNow.Minute == GetCurrentTime().Minute && timeNow.Second == GetCurrentTime().Second)
                {
                    continue;
                }

                Process process = StartProcess("netsh.exe", " wlan show interfaces");

                var processOutput = GetProcessOutput(process);

                var processOutputArray = ProcessOutputToArray(processOutput);

                ShowSignalFromProcessOutput(processOutputArray);

                process.WaitForExit();
                process.Close();
                timeNow = GetCurrentTime();
            }

            DisplayMessage("\n\nPress any key to exit.");
            Console.ReadLine();
        }

        private static DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        private static string GetProcessOutput(Process process)
        {
            StreamReader processReader = process.StandardOutput;
            return processReader.ReadToEnd();
        }

        private static string[] ProcessOutputToArray(string processOutput)
        {
            return processOutput.Split('\n');
        }

        private static void ShowSignalFromProcessOutput(string[] array)
        {
            foreach (var item in array)
            {
                if (item.Contains("Signal"))
                {
                    string signalStrength = ExtractSignalStrength(item);

                    decimal dBm = RSSIIndBm(signalStrength);
                    
                    DisplayMessage($"{GetCurrentTime()} \t {signalStrength } \t\t\t\t { dBm } ");

                    break;
                }
            }
        }

        private static string ExtractSignalStrength(string item)
        {
            return item.Substring(item.Length - 6, 3);
        }

        private static void DisplayMessage(string msgToShow)
        {
            Console.WriteLine(msgToShow);
        }

        private static decimal RSSIIndBm(string signalStrength)
        {
            int parsedToInt = 0;
            if (int.TryParse(signalStrength, out parsedToInt))
            {
                return parsedToInt / 2 - 100;
            }
            return 0;
        }

        private static Process StartProcess(string processFileName, string processArguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = processFileName;
            process.StartInfo.Arguments = processArguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            return process;
        }
    }
}
