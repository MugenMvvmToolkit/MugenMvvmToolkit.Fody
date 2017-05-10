using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using MugenMvvmToolkit.Fody.TestAssembly;

namespace MugenMvvmToolkit.Fody.Test
{
    [TestClass]
    public class WeaverTests
    {
        #region Fields

        private Assembly _assembly;

        #endregion

        #region Methods

        [TestInitialize]
        public void Setup()
        {
            string projectPath =
                Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                    @"..\..\..\MugenMvvmToolkit.Fody.TestAssembly\MugenMvvmToolkit.Fody.TestAssembly.csproj"));
            var assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath),
                @"bin\Debug\MugenMvvmToolkit.Fody.TestAssembly.dll");
#if !DEBUG
            assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif
            var newAssemblyPath = assemblyPath.Replace(".dll", "2.dll");
            File.Copy(assemblyPath, newAssemblyPath, true);

            using (ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(newAssemblyPath))
            {
                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition,
                    AssemblyResolver = new DefaultAssemblyResolver(),
                    LogInfo = s => Console.WriteLine(s),
                    LogWarning = s => Console.WriteLine(s)
                };

                weavingTask.Execute();

                newAssemblyPath = assemblyPath.Replace(".dll", "3.dll");
                moduleDefinition.Write(newAssemblyPath);
            }

            _assembly = Assembly.LoadFile(newAssemblyPath);
        }

        [TestMethod]
        public void ValidateAsyncMethods()
        {
            Type type = _assembly.GetType(typeof(TestClass).FullName);
            var instance = (dynamic)Activator.CreateInstance(type);

            var resetEvent = new ManualResetEvent(false);
            instance.Run(resetEvent);
            resetEvent.WaitOne(1000);
        }

        #endregion
    }
}