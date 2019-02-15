//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Web.Management.PHP.Setup
{

    public static class InstallUtil
    {
        public static void AddUIModuleProvider(string name, string type)
        {
            Execute(name, type);
        }

        private static void Execute(string name, string type)
        {
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
                            "Framework",
                            iisVersion == 7 ? "v3.5" : "v4.0.30319"))));
            var assembly = Assembly.GetExecutingAssembly();
            RegisterIIS(framework, assembly, name, type);
            RegisterSnapin(framework, assembly, type);
        }

        private static void RegisterIIS(string framework, Assembly assembly, string name, string type)
        {
            var compiler = Path.Combine(framework, "csc.exe");

            var mwa = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Path.Combine(
                    "inetsrv",
                    "Microsoft.Web.Administration.dll"));

            var source = Path.GetTempFileName();
            var program = source + ".iis.exe";
            using (var stream = assembly.GetManifestResourceStream("Web.Management.PHP.Setup.Program.cs"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                File.WriteAllText(source, content);
            }

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = compiler,
                    Arguments = string.Format("/r:{0} /out:\"{1}\" \"{2}\"", mwa, program, source),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            File.Delete(source);

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = type == null ? string.Format("/u \"{0}\"", name) : string.Format("/i \"{0}\" \"{1}\"", name, type),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            File.Delete(program);
        }

        private static void RegisterSnapin(string framework, Assembly assembly, string type)
        {
            var compiler = Path.Combine(framework, "csc.exe");

            var source = Path.GetTempFileName();
            var program = source + ".powershell.exe";
            using (var stream = assembly.GetManifestResourceStream("Web.Management.PHP.Setup.Program2.cs"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                File.WriteAllText(source, content);
            }

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = compiler,
                    Arguments = string.Format("/out:\"{0}\" \"{1}\"", program, source),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            File.Delete(source);

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = type == null ? string.Format("/u \"{0}\"", assembly.Location) : string.Format("/i \"{0}\"", assembly.Location),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            File.Delete(program);
        }

        /// <summary> 
        /// Removes the specified UI Module by name 
        /// </summary> 
        public static void RemoveUIModuleProvider(string name)
        {
            Execute(name, null);
        }
    }
}
