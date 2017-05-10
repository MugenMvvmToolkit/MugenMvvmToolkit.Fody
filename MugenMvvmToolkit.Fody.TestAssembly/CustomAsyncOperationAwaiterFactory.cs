using System;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class CustomAsyncOperationAwaiterFactory : IOperationCallbackFactory
    {
        #region Implementation of IOperationCallbackFactory

        public IAsyncOperationAwaiter CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            return new CustomAwaiter();
        }

        public IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>(IAsyncOperation<TResult> operation, IDataContext context)
        {
            return new CustomAwaiter<TResult>();
        }

        public ISerializableCallback CreateSerializableCallback(Delegate @delegate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}