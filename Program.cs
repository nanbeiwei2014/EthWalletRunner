using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EthWalletRunner
{
    class Program
    {
        static ConcurrentQueue<String> g_Lines = new ConcurrentQueue<string>();

        static void Main(string[] args)
        {
            String regsFileName = "patterns.txt";
            String settingsFileName = "settings.txt";
            String ethWalletFileName = "";

            String[] patterns = File.ReadAllLines(regsFileName);
            String[] settings = File.ReadAllLines(settingsFileName);

            foreach(String setting in settings)
            {
                String[] line = setting.Split('=');
                if (line[0] == "ethwallet")
                    ethWalletFileName = line[1];
            }

            Utilities.system.Execute.receivedDataEvent += Execute_receivedDataEvent;
            Utilities.system.Execute.g_Async = true;
            Utilities.system.Execute.g_CreateNoWindow = false;

            Console.WriteLine("Starting Eth Wallet...");
            Utilities.system.Execute.runCmd("C:\\Windows\\System32\\taskkill", "/F /IM \"Ethereum Wallet.exe\"");
            Utilities.system.Execute.wait();
            Utilities.system.Execute.runCmd(ethWalletFileName);
            while (true)
            {
                if(g_Lines.Count > 0)
                {
                    String line = "";
                    g_Lines.TryDequeue(out line);
                    foreach (String rp in patterns)
                    {
                        if (String.IsNullOrEmpty(line))
                            break;
                        Regex rx = new Regex(rp, RegexOptions.IgnoreCase);
                        Match mt = rx.Match(line);
                        if (mt.Success)
                        {
                            Console.WriteLine("Stopping Eth Wallet...");
                            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
                            Utilities.system.Execute.terminate();
                            Utilities.system.Execute.runCmd("C:\\Windows\\System32\\taskkill", "/IM \"Ethereum Wallet.exe\"");
                            Utilities.system.Execute.wait();
                            while (g_Lines.TryDequeue(out line));
                            Task.Delay(TimeSpan.FromMinutes(5)).Wait();
                            Console.WriteLine("Starting Eth Wallet...");
                            Utilities.system.Execute.runCmd(ethWalletFileName);
                            break;
                        }
                    }
                }
                Task.Delay(120).Wait();
            }
        }

        private static void Execute_receivedDataEvent(string data)
        {
            Console.WriteLine(data);
            g_Lines.Enqueue(data);
        }
    }
}
