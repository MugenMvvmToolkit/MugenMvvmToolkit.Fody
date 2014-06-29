#region Copyright
// ****************************************************************************
// <copyright file="ModuleWeaver.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace MugenMvvmToolkit.Fody
{
    public sealed class ModuleWeaver
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleWeaver"/> class.
        /// </summary>
        public ModuleWeaver()
        {
            LogInfo = s => { };
        }

        #endregion

        #region Properties

        // Will log an informational message to MSBuild
        public Action<string> LogInfo { get; set; }

        // An instance of Mono.Cecil.ModuleDefinition for processing
        public ModuleDefinition ModuleDefinition { get; set; }

        #endregion

        #region Methods

        public void Execute()
        {
            var types = new List<TypeDefinition>();
            CollectAsyncStateMachine(ModuleDefinition.Types, types);
            foreach (var typeDefinition in types)
                foreach (var methodDefinition in typeDefinition.Methods)
                    TryUpdateMethod(methodDefinition);
        }

        private void TryUpdateMethod(MethodDefinition method)
        {
            method.Body.SimplifyMacros();

            var instructions = method.Body.Instructions;
            for (var index = 0; index < instructions.Count; index++)
            {
                var line = instructions[index];
                if (line.OpCode != OpCodes.Call)
                    continue;

                var oldMethod = line.Operand as MethodReference;
                if (oldMethod == null)
                    continue;
                MethodReference newMethod = oldMethod
                    .DeclaringType
                    .Resolve()
                    .GetMethods()
                    .FirstOrDefault(definition => definition.Name == oldMethod.Name && definition.Parameters.Count == 2 &&
                                      definition.Parameters[1].ParameterType.FullName == Constants.AsyncStateMachineIntefaceFullName &&
                                      definition.HasGenericParameters == oldMethod.IsGenericInstance);
                if (newMethod == null)
                    continue;
                if (newMethod.HasGenericParameters)
                {
                    var genericInstance = (IGenericInstance)oldMethod;
                    if (genericInstance.GenericArguments.Count != newMethod.GenericParameters.Count)
                        continue;
                    var genericMethod = new GenericInstanceMethod(newMethod);
                    foreach (var argument in genericInstance.GenericArguments)
                        genericMethod.GenericArguments.Add(argument);
                    newMethod = genericMethod;
                }

                instructions.RemoveAt(index);
                instructions.Insert(index, Instruction.Create(OpCodes.Ldarg_0));
                index++;
                if (method.DeclaringType.IsValueType)
                {
                    instructions.Insert(index, Instruction.Create(OpCodes.Ldobj, method.DeclaringType));
                    index++;
                    instructions.Insert(index, Instruction.Create(OpCodes.Box, method.DeclaringType));
                    index++;
                }
                instructions.Insert(index, Instruction.Create(OpCodes.Call, newMethod));
                index++;
                LogInfo(string.Format("AsyncStateMachine {0} was updated", method.DeclaringType.Name));
            }
            method.Body.OptimizeMacros();
        }

        private static void CollectAsyncStateMachine(IEnumerable<TypeDefinition> typeDefinitions, List<TypeDefinition> types)
        {
            foreach (TypeDefinition typeDefinition in typeDefinitions)
            {
                CollectAsyncStateMachine(typeDefinition.NestedTypes, types);
                if (typeDefinition.IsAsyncStateMachine())
                    types.Add(typeDefinition);
            }
        }

        #endregion
    }
}