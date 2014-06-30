using System;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public static class Extensions
    {
        public static CustomAwaiter GetAwaiter(this Action action)
        {
            return new CustomAwaiter();
        }

        public static CustomAwaiter<TResult> GetAwaiter<TResult>(this Func<TResult> func)
        {
            return new CustomAwaiter<TResult>();
        }
    }
}