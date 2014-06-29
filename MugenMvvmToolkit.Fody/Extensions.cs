#region Copyright
// ****************************************************************************
// <copyright file="Extensions.cs">
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

using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace MugenMvvmToolkit.Fody
{
    public static class Extensions
    {
        #region Methods

        public static bool IsAsyncStateMachine(this TypeDefinition typeDefinition)
        {
            return typeDefinition.HasAsyncStateMachineInterface() &&
                   typeDefinition.IsCompilerGenerated();
        }

        public static bool IsCompilerGenerated(this TypeDefinition typeDefinition)
        {
            return typeDefinition
                .CustomAttributes
                .Any(x => x.Constructor.DeclaringType.FullName == typeof (CompilerGeneratedAttribute).FullName);
        }

        private static bool HasAsyncStateMachineInterface(this TypeDefinition typeDefinition)
        {
            return typeDefinition.Interfaces.Any(x => x.FullName == Constants.AsyncStateMachineIntefaceFullName);
        }

        #endregion
    }
}