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
        private string _assemblyPath;
        private string _newAssemblyPath;

        #endregion

        #region Methods

        [TestInitialize]
        public void Setup()
        {
            string projectPath =
                Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                    @"..\..\..\MugenMvvmToolkit.Fody.TestAssembly\MugenMvvmToolkit.Fody.TestAssembly.csproj"));
            _assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath),
                @"bin\Debug\MugenMvvmToolkit.Fody.TestAssembly.dll");
#if !DEBUG
            _assemblyPath = _assemblyPath.Replace("Debug", "Release");
#endif
            _newAssemblyPath = _assemblyPath.Replace(".dll", "2.dll");
            File.Copy(_assemblyPath, _newAssemblyPath, true);

            ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(_newAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new DefaultAssemblyResolver(),
                LogInfo = s => Console.WriteLine(s),
                LogWarning = s => Console.WriteLine(s)
            };

            weavingTask.Execute();
            moduleDefinition.Write(_newAssemblyPath);

            _assembly = Assembly.LoadFile(_newAssemblyPath);
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