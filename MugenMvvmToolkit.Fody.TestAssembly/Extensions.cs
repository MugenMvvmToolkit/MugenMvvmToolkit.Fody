using System;
using System.Runtime.CompilerServices;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public static class Extensions
    {
        public static CustomAwaiter GetAwaiter(this Action action)
        {
            throw new NotSupportedException();
        }

        public static CustomAwaiter GetAwaiter(this Action action, IAsyncStateMachine stateMachine)
        {
            return new CustomAwaiter();
        }

        public static CustomAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> func)
        {
            throw new NotSupportedException();
        }

        public static CustomAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> func,
            IAsyncStateMachine stateMachine)
        {
            return new CustomAwaiter<TResult>();
        }
    }
}