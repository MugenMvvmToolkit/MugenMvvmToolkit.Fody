using System;
using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class TestClass
    {
        #region Methods

        public async void Run(ManualResetEvent resetEvent)
        {
            await AsyncMethodNonGeneric();
            await AsyncMethodGeneric();
            resetEvent.Set();
        }

        private async Task AsyncMethodNonGeneric()
        {
            Action action = () => { };
            await action;
        }

        private async Task<bool> AsyncMethodGeneric()
        {
            Func<bool> action = () => true;
            return await action;
        }

        #endregion
    }
}