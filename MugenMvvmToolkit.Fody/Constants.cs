#region Copyright

// ****************************************************************************
// <copyright file="Constants.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit.Fody</project>
// <web>http://vvson.net</web>
// <license>
// See license.txt in this solution or http://mugenmvvmtoolkit.codeplex.com/license
// </license>
// ****************************************************************************

#endregion

namespace MugenMvvmToolkit.Fody
{
    public static class Constants
    {
        public const string MugenMvvmToolkitAssemblyName = "MugenMvvmToolkit";

        public const string AsyncStateMachineAwareFullName = "MugenMvvmToolkit.Interfaces.Callbacks.IAsyncStateMachineAware";

        public const string AsyncStateMachineIntefaceFullName = "System.Runtime.CompilerServices.IAsyncStateMachine";

        public const string SetStateMachineMethodName = "SetStateMachine";

        public const string MoveNextMethodName = "MoveNext";

        public const string AwaiterName = "$awaiter";
    }
}