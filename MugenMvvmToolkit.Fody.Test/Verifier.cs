using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MugenMvvmToolkit.Fody.Test
{
    public static class Verifier
    {
        #region Methods

        public static void Verify(string beforeAssemblyPath, string afterAssemblyPath)
        {
            string before = Validate(beforeAssemblyPath);
            string after = Validate(afterAssemblyPath);
            string message = string.Format("Failed processing {0}\r\n{1}", Path.GetFileName(afterAssemblyPath), after);
            Assert.AreEqual(TrimLineNumbers(before), TrimLineNumbers(after), message);
        }

        private static string Validate(string assemblyPath2)
        {
            string exePath = GetPathToPEVerify();
            if (!File.Exists(exePath))
            {
                return string.Empty;
            }
            Process process = Process.Start(new ProcessStartInfo(exePath, "\"" + assemblyPath2 + "\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process.WaitForExit(10000);
            return process.StandardOutput.ReadToEnd().Trim().Replace(assemblyPath2, "");
        }

        private static string GetPathToPEVerify()
        {
            string exePath =
                Environment.ExpandEnvironmentVariables(
                    @"%programfiles(x86)%\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\PEVerify.exe");

            if (!File.Exists(exePath))
            {
                exePath =
                    Environment.ExpandEnvironmentVariables(
                        @"%programfiles(x86)%\Microsoft SDKs\Windows\v8.0A\Bin\NETFX 4.0 Tools\PEVerify.exe");
            }
            return exePath;
        }

        private static string TrimLineNumbers(string foo)
        {
            return Regex.Replace(foo, @"0x.*]", "");
        }

        #endregion
    }
}