using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class TestClass
    {
        #region Constructors

        static TestClass()
        {
            ServiceProvider.AsyncOperationAwaiterFactory = new CustomAsyncOperationAwaiterFactory();
        }

        #endregion

        #region Methods

        public async void Run(ManualResetEvent resetEvent)
        {
            await AsyncMethodNonGeneric();
            await AsyncMethodGeneric();
            await AsyncMugenAwaiterMethod();
            await AsyncMugenAwaiterMethodGeneric();
            if (CustomAwaiter.Awaiters.Count == 0)
                throw new InvalidOperationException("The Awaiters collection is empty");
            foreach (CustomAwaiter customAwaiter in CustomAwaiter.Awaiters)
            {
                if (customAwaiter.StateMachine == null)
                    throw new InvalidOperationException("The StateMachine is null");
            }
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

        private async Task AsyncMugenAwaiterMethod()
        {
            IAsyncOperation<bool> operation = new AsyncOperation<bool>();
            await operation;
        }

        private async Task<bool> AsyncMugenAwaiterMethodGeneric()
        {
            IAsyncOperation<bool> operation = new AsyncOperation<bool>();
            return await operation;
        }

        #endregion
    }
}