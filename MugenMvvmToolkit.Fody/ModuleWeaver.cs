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
        #region Fields

        private TypeReference _asyncStateMachineAwareType;
        private MethodReference _setStateMachineMethod;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleWeaver"/> class.
        /// </summary>
        public ModuleWeaver()
        {
            LogInfo = s => { };
            LogWarning = s => { };
        }

        #endregion

        #region Properties

        public Action<string> LogInfo { get; set; }

        public Action<string> LogWarning { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }

        public IAssemblyResolver AssemblyResolver { get; set; }

        #endregion

        #region Methods

        public void Execute()
        {
            var definition = AssemblyResolver.Resolve(Constants.MugenMvvmToolkitAssemblyName);
            if (definition == null)
            {
                LogWarning(string.Format("The {0} is not referenced to a project {1}",
                    Constants.MugenMvvmToolkitAssemblyName, ModuleDefinition.Name));
                return;
            }
            TypeReference type = definition.MainModule.Types.FirstOrDefault(typeDefinition => typeDefinition.FullName == Constants.AsyncStateMachineAwareFullName);
            if (type == null)
            {
                LogWarning(string.Format("The type {0} was not found", Constants.AsyncStateMachineAwareFullName));
                return;
            }
            var resolveType = type.Resolve();
            if (resolveType == null)
            {
                LogWarning(string.Format("The type {0} was not found", Constants.AsyncStateMachineAwareFullName));
                return;
            }
            _asyncStateMachineAwareType = ModuleDefinition.Import(type);
            var setStateMachineMethod = resolveType.Methods
                .FirstOrDefault(method => method.Name == Constants.SetStateMachineMethodName && method.Parameters.Count == 1 &&
                          method.Parameters[0].ParameterType.FullName == Constants.AsyncStateMachineIntefaceFullName);
            if (setStateMachineMethod == null)
            {
                LogWarning(string.Format("The method {0} was not found", Constants.AsyncStateMachineIntefaceFullName));
                return;
            }
            _setStateMachineMethod = ModuleDefinition.Import(setStateMachineMethod);

            var types = new List<TypeDefinition>();
            CollectAsyncStateMachine(ModuleDefinition.Types, types);
            foreach (var typeDefinition in types)
                foreach (var methodDefinition in typeDefinition.Methods)
                    TryUpdateMethod(methodDefinition);
        }

        private void TryUpdateMethod(MethodDefinition method)
        {
            if (method.Name != Constants.SetStateMachineMethodName || method.Parameters.Count != 1 ||
                method.Parameters[0].ParameterType.FullName != Constants.AsyncStateMachineIntefaceFullName)
                return;
            foreach (var field in method.DeclaringType.Fields.Where(definition => definition.Name.Contains("$awaiter")))
            {

                if (field.FieldType.IsValueType)
                    LogInfo(string.Format("The awaiter field on type '{0}' is a value type '{1}'", method.DeclaringType,
                        field));
                else
                    UpdateStateMachine(method, field);
            }
        }

        private void UpdateStateMachine(MethodDefinition method, FieldDefinition field)
        {
            method.Body.SimplifyMacros();
            var variable = new VariableDefinition(_asyncStateMachineAwareType);
            method.Body.Variables.Add(variable);
            var instructions = method.Body.Instructions;
            var index = instructions.Count - 1;
            var returnInst = instructions[index];
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, field));
            instructions.Insert(index++, Instruction.Create(OpCodes.Isinst, _asyncStateMachineAwareType));
            instructions.Insert(index++, Instruction.Create(OpCodes.Stloc, variable));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, variable));
            instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

            instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, variable));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_1));
            instructions.Insert(index, Instruction.Create(OpCodes.Callvirt, _setStateMachineMethod));

            method.Body.OptimizeMacros();
            LogInfo(string.Format("AsyncStateMachine {0} was updated", method.DeclaringType.Name));
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