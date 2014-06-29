using System;
using System.Runtime.CompilerServices;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class CustomAwaiter : INotifyCompletion
    {
        #region Awaiter implementation

        public bool IsCompleted
        {
            get { return true; }
        }

        public void OnCompleted(Action continuation)
        {
        }

        public void GetResult()
        {
        }

        #endregion
    }

    public class CustomAwaiter<TResult> : CustomAwaiter
    {
        public new TResult GetResult()
        {
            return default(TResult);
        }
    }
}