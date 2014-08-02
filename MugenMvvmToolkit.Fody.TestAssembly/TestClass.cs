using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Utils;

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
            await DoubleAsyncMugenAwaiterMethod();
            await DoubleAsyncMugenAwaiterMethod();
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

        private async Task DoubleAsyncMugenAwaiterMethod()
        {
            var b = await new Func<bool>(() => true) || await MvvmUtils.TrueTaskResult;
            if (b)
            {
                IAsyncOperation<bool> operation1 = new AsyncOperation<bool>();
                await operation1;
            }

            IAsyncOperation<bool> operation2 = new AsyncOperation<bool>();
            await operation2;
        }

        private async Task<bool> DoubleAsyncMugenAwaiterMethodGeneric()
        {
            var b = await new Func<bool>(() => true) || await MvvmUtils.TrueTaskResult;
            bool result = false;
            if (b)
            {
                IAsyncOperation<bool> operation1 = new AsyncOperation<bool>();
                result = await operation1;
            }

            IAsyncOperation<bool> operation2 = new AsyncOperation<bool>();
            return await operation2 || result;
        }

        #endregion
    }
}