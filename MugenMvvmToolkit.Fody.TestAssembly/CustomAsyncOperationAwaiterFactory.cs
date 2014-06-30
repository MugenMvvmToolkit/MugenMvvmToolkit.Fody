using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Fody.TestAssembly
{
    public class CustomAsyncOperationAwaiterFactory : IAsyncOperationAwaiterFactory
    {
        #region Implementation of IAsyncOperationAwaiterFactory

        /// <summary>
        ///     Creates an instance of <see cref="T:MugenMvvmToolkit.Interfaces.Callbacks.IAsyncOperationAwaiter" />.
        /// </summary>
        public IAsyncOperationAwaiter CreateAwaiter(IAsyncOperation operation, bool isSerializable, IDataContext context)
        {
            return new CustomAwaiter();
        }

        /// <summary>
        ///     Creates an instance of <see cref="T:MugenMvvmToolkit.Interfaces.Callbacks.IAsyncOperationAwaiter`1" />.
        /// </summary>
        public IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>(IAsyncOperation<TResult> operation,
            bool isSerializable, IDataContext context)
        {
            return new CustomAwaiter<TResult>();
        }

        #endregion
    }
}