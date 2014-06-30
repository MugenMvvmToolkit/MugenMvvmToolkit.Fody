using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class CustomAwaiter : IAsyncOperationAwaiter, IAsyncStateMachineAware
    {
        #region Constructors

        static CustomAwaiter()
        {
            Awaiters = new List<CustomAwaiter>();
        }

        public CustomAwaiter()
        {
            Awaiters.Add(this);
        }

        #endregion

        #region Properties

        public static List<CustomAwaiter> Awaiters { get; private set; }

        public IAsyncStateMachine StateMachine { get; private set; }

        #endregion

        #region Awaiter implementation

        public bool IsCompleted { get; private set; }

        public void OnCompleted(Action continuation)
        {
            IsCompleted = true;
            continuation();
        }

        public void GetResult()
        {
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        #endregion
    }

    public class CustomAwaiter<TResult> : CustomAwaiter, IAsyncOperationAwaiter<TResult>
    {
        public new TResult GetResult()
        {
            return default(TResult);
        }
    }
}