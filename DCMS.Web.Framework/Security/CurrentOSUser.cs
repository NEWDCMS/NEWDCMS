using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

namespace DCMS.Web.Framework.Security
{
    /// <summary>
    /// 表示有关当前OS用户的信息
    /// </summary>
    public static class CurrentOSUser
    {

        static CurrentOSUser()
        {
            Name = Environment.UserName;

            DomainName = Environment.UserDomainName;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    PopulateWindowsUser();
                    break;
                case PlatformID.Unix:
                    PopulateLinuxUser();
                    break;
                default:
                    UserId = Name;
                    Groups = new List<string>();
                    break;
            }
        }

        /// <summary>
        ///填充有关Windows用户的信息
        /// </summary>
        public static void PopulateWindowsUser()
        {
            Groups = WindowsIdentity.GetCurrent().Groups?.Select(p => p.Value).ToList();
            UserId = Name;
        }

        /// <summary>
        /// 填充有关linux用户的信息
        /// </summary>
        public static void PopulateLinuxUser()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "sh",
                    Arguments = "-c \" id -u ; id -G \""
                }
            };

            process.Start();
            process.WaitForExit();

            var res = process.StandardOutput.ReadToEnd();

            var respars = res.Split("\n");

            UserId = respars[0];
            Groups = respars[1].Split(" ").ToList();
        }


        /// <summary>
        /// 返回用户名
        /// </summary>
        public static string Name { get; }

        /// <summary>
        /// 返回Windows的用户域名或Linux的组
        /// </summary>
        public static string DomainName { get; }

        /// <summary>
        /// 返回用户组
        /// </summary>
        public static List<string> Groups { get; private set; }

        /// <summary>
        /// 返回Windows的用户名或Linux的用户ID，例如1001
        /// </summary>
        public static string UserId { get; private set; }

        /// <summary>
        /// 返回完整的用户名
        /// </summary>
        public static string FullName => $@"{DomainName}\{Name}";

    }
}