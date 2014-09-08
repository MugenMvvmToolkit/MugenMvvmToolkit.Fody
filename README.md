![MugenMvvmToolkit](https://raw.githubusercontent.com/MugenMvvmToolkit/MugenMvvmToolkit/master/logo_horizontal.png)

----------
## This is an add-in for [Fody](https://github.com/Fody/Fody/) 
Adds support for save the state of asynchronous tasks for WP and WinRT platforms. Updates all `IAsyncStateMachine` in the code so that they passed itself to the `IAsyncStateMachineAware` interface.

### Your Code

	[CompilerGenerated]
	[StructLayout(LayoutKind.Auto)]
	private struct <TestMethod>d__0 : IAsyncStateMachine
	{
		public int <>1__state;
		public AsyncVoidMethodBuilder <>t__builder;
		private object <>u__$awaiter1;
		private object <>t__stack;
		void IAsyncStateMachine.MoveNext()
		{
			try
			{
				int num = this.<>1__state;
				IAsyncOperationAwaiter<bool> asyncOperationAwaiter;
				if (num != 0)
				{
					asyncOperationAwaiter = Program.Get().GetAwaiter<bool>();
					if (!asyncOperationAwaiter.IsCompleted)
					{
						this.<>1__state = 0;
						this.<>u__$awaiter1 = asyncOperationAwaiter;
						this.<>t__builder.AwaitOnCompleted<IAsyncOperationAwaiter<bool>, Program.<TestMethod>d__0>(ref asyncOperationAwaiter, ref this);
						return;
					}
				}
				else
				{
					asyncOperationAwaiter = (IAsyncOperationAwaiter<bool>)this.<>u__$awaiter1;
					this.<>u__$awaiter1 = null;
					this.<>1__state = -1;
				}
				asyncOperationAwaiter.GetResult();
				asyncOperationAwaiter = null;
			}
			catch (Exception exception)
			{
				this.<>1__state = -2;
				this.<>t__builder.SetException(exception);
				return;
			}
			this.<>1__state = -2;
			this.<>t__builder.SetResult();
		}
		[DebuggerHidden]
		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine param0)
		{
			this.<>t__builder.SetStateMachine(param0);
		}
	}
	


### What gets compiled

	[CompilerGenerated]
	[StructLayout(LayoutKind.Auto)]
	private struct <TestMethod>d__0 : IAsyncStateMachine
	{
		public int <>1__state;
		public AsyncVoidMethodBuilder <>t__builder;
		private object <>u__$awaiter1;
		private object <>t__stack;
		private IAsyncStateMachine $_self_;
		void IAsyncStateMachine.MoveNext()
		{
			try
			{
				int num = this.<>1__state;
				IAsyncOperationAwaiter<bool> asyncOperationAwaiter;
				if (num != 0)
				{
					asyncOperationAwaiter = Program.Get().GetAwaiter<bool>();
					if (!asyncOperationAwaiter.IsCompleted)
					{
						this.<>1__state = 0;
						this.<>u__$awaiter1 = asyncOperationAwaiter;
						IAsyncStateMachineAware asyncStateMachineAware = this.<>u__$awaiter1 as IAsyncStateMachineAware;
						if (asyncStateMachineAware != null && this.$_self_ != null)
						{
							asyncStateMachineAware.SetStateMachine(this.$_self_);
						}
						this.<>t__builder.AwaitOnCompleted<IAsyncOperationAwaiter<bool>, Program.<TestMethod>d__0>(ref asyncOperationAwaiter, ref this);
						return;
					}
				}
				else
				{
					asyncOperationAwaiter = (IAsyncOperationAwaiter<bool>)this.<>u__$awaiter1;
					this.<>u__$awaiter1 = null;
					this.<>1__state = -1;
				}
				asyncOperationAwaiter.GetResult();
				asyncOperationAwaiter = null;
			}
			catch (Exception exception)
			{
				this.<>1__state = -2;
				this.<>t__builder.SetException(exception);
				return;
			}
			this.<>1__state = -2;
			this.<>t__builder.SetResult();
		}
		[DebuggerHidden]
		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine param0)
		{
			this.<>t__builder.SetStateMachine(param0);
			this.$_self_ = param0;
			IAsyncStateMachineAware asyncStateMachineAware = this.<>u__$awaiter1 as IAsyncStateMachineAware;
			if (asyncStateMachineAware != null)
			{
				asyncStateMachineAware.SetStateMachine(param0);
			}
		}
	}
