//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Web.Management.PHP
{

    internal static class Helper
    {

        internal static bool Browse(string url)
        {
            if (url != null)
            {
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Process.Start(url);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
                
            return false;
        }

        internal static string EnsureTrailingSlash(string path)
        {
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path + "/";
            }

            return path;
        }

        internal static string GetURLFromBinding(string serverName, string bindingProtocol, string bindingInformation)
        {
            string ipAddress = String.Empty;
            string port = String.Empty;
            string hostHeader = String.Empty;

            string[] values = bindingInformation.Split(':');
            if (values.Length == 3)
            {
                ipAddress = values[0];
                port = values[1];
                hostHeader = values[2];
            }
            else
            {
                // if it's a ipv6 address, the bindingInformation can look like:
                //"[3ffe:8311:ffff:f70f:0:5efe:172.30.188.251]:80:hostheader"
                if (values.Length > 2)
                {
                    // look for the last delimiter for the hostheader
                    int lastOne = bindingInformation.LastIndexOf(':');

                    // look for the last delimiter for the port
                    string tempString = bindingInformation.Substring(0, lastOne);
                    int secondLastOne = tempString.LastIndexOf(':');

                    // everything before that colon is the ip address
                    ipAddress = bindingInformation.Substring(0, secondLastOne);
                    port = values[values.Length - 2];
                    hostHeader = values[values.Length - 1];
                }
            }

            if (port == "80")
            {
                port = null;
            }

            // check if the binding has a hostheader, if it does than just use that
            // as the ip address
            if (!string.IsNullOrEmpty(hostHeader))
            {
                ipAddress = hostHeader;
            }
            else
            {
                // host header wasnt set, so lets see if there is any specified ip address
                // if there is, then just use that as the ip address.
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = serverName;
                }
                else
                {
                    if (String.Equals(ipAddress, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = serverName;
                    }
                }
            }
            if (port != null)
            {
                return bindingProtocol + "://" + ipAddress + ":" + port + "/";
            }

            return bindingProtocol + "://" + ipAddress + "/";
        }

        /// <summary>
        /// returns a list of URLs based on the bindings provided and the server name.
        ///  
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="bindings">Arraylist of string[] {bindingProtocol, bindingInformation}</param>
        /// <param name="relativePath"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static List<string> GetUrlListFromBindings(string serverName, ArrayList bindings, string relativePath)
        {
            var urls = new List<string>();

            foreach (string[] b in bindings)
            {
                var url = GetURLFromBinding(serverName, b[0], b[1]);
                try
                {
                    var siteUri = new Uri(url);
                    var uri = new Uri(siteUri, relativePath);

                    var absoluteUrl = uri.AbsoluteUri;
                    urls.Add(EnsureTrailingSlash(absoluteUrl));
                }
                catch
                {
                }
            }

            return urls;
        }
    }
}