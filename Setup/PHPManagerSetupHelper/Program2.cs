using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Web.Management.PHP.Setup
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            bool add;
            if (args[0] == "/i")
            {
                add = true;
            }
            else if (args[0] == "/u")
            {
                add = false;
            }
            else
            {
                return;
            }

            var location = args[1];

            var registry = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W3SVC\Parameters");
            if (registry == null)
            {
                return;
            }

            var iisVersion = (int)registry.GetValue("MajorVersion", 6);
            var framework = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Path.Combine(
                    "..",
                    Path.Combine(
                        "Microsoft.NET",
                        Path.Combine(
                            IntPtr.Size == 8 ? "Framework64" : "Framework",
                            iisVersion == 7 ? "v2.0.50727" : "v4.0.30319"))));
            var process = Path.Combine(framework, "InstallUtil.exe");
            var file = Path.Combine(Path.GetDirectoryName(location), "Web.Management.PHP.PowerShell.dll");
            if (add)
            {
                AddSnapin(process, file);
            }
            else
            {
                RemoveSnapin(process, file);
            }
        }

        public static void AddSnapin(string process, string file)
        {
            using (var p = Process.Start(process, file))
            {
                p.WaitForExit();
            }
        }

        public static void RemoveSnapin(string process, string file)
        {
            using (var p = Process.Start(process, string.Format("/u {0}", file)))
            {
                p.WaitForExit();
            }
        }
    }
}
