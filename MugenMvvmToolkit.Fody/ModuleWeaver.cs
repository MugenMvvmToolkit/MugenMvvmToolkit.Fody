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
            var definition = AssemblyResolver.Resolve(new AssemblyNameReference(Constants.MugenMvvmToolkitAssemblyName, new Version()));
            if (definition == null)
            {
                LogWarning($"The {Constants.MugenMvvmToolkitAssemblyName} is not referenced to a project {ModuleDefinition.Name}");
                return;
            }
            TypeReference type = definition.MainModule.Types.FirstOrDefault(typeDefinition => typeDefinition.FullName == Constants.AsyncStateMachineAwareFullName);
            if (type == null)
            {
                LogWarning($"The type {Constants.AsyncStateMachineAwareFullName} was not found");
                return;
            }
            var resolveType = type.Resolve();
            if (resolveType == null)
            {
                LogWarning($"The type {Constants.AsyncStateMachineAwareFullName} was not found");
                return;
            }
            _asyncStateMachineAwareType = ModuleDefinition.ImportReference(type);
            var setStateMachineMethod = resolveType.Methods
                .FirstOrDefault(method => method.Name == Constants.SetStateMachineMethodName && method.Parameters.Count == 1 &&
                          method.Parameters[0].ParameterType.FullName == Constants.AsyncStateMachineIntefaceFullName);
            if (setStateMachineMethod == null)
            {
                LogWarning($"The method {Constants.SetStateMachineMethodName} was not found");
                return;
            }
            _setStateMachineMethod = ModuleDefinition.ImportReference(setStateMachineMethod);

            var types = new List<TypeDefinition>();
            CollectAsyncStateMachine(ModuleDefinition.Types, types);
            foreach (var typeDefinition in types)
                TryUpdateStateMachine(typeDefinition);
        }

        private void TryUpdateStateMachine(TypeDefinition type)
        {
            FieldDefinition selfField = null;
            //Old compiler awaiter name name is $awaiter            
            if (type.Fields.Any(definition => definition.Name.Contains(Constants.AwaiterName)))
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == Constants.SetStateMachineMethodName)
                    {
                        if (method.Parameters.Count != 1 ||
                            method.Parameters[0].ParameterType.FullName != Constants.AsyncStateMachineIntefaceFullName)
                            continue;
                    }
                    else
                    {
                        if (method.Name != Constants.MoveNextMethodName || method.Parameters.Count != 0)
                            continue;
                        GenerateSelfField(ref selfField, type);
                        UpdateMoveNextMethodOldCompiler(method, type, ref selfField);
                        continue;
                    }

                    var fields = method.DeclaringType.Fields.Where(definition => definition.Name.Contains(Constants.AwaiterName)).ToList();
                    foreach (var field in fields)
                    {
                        if (field.FieldType.IsValueType)
                        {
                            LogInfo($"The awaiter field on type '{method.DeclaringType}' is a value type '{field}'");
                            continue;
                        }
                        //need to make it public in order to use with xamarin
                        field.Attributes = FieldAttributes.Public;
                        GenerateSelfField(ref selfField, type);
                        UpdateStateMachineMethod(method, field, type, ref selfField);
                    }
                }
            }
            else
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == Constants.SetStateMachineMethodName)
                    {
                        if (method.Parameters.Count != 1 ||
                            method.Parameters[0].ParameterType.FullName != Constants.AsyncStateMachineIntefaceFullName)
                            continue;
                    }
                    else
                    {
                        if (method.Name != Constants.MoveNextMethodName || method.Parameters.Count != 0)
                            continue;
                        GenerateSelfField(ref selfField, type);
                        UpdateMoveNextMethodNewCompiler(method, type, ref selfField);
                        continue;
                    }

                    var fields = method.DeclaringType.Fields.Where(definition => definition.Name.StartsWith(Constants.AwaiterNameNew, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var field in fields)
                    {
                        if (field.FieldType.IsValueType)
                        {
                            LogInfo($"The awaiter field on type '{method.DeclaringType}' is a value type '{field}'");
                            continue;
                        }
                        //need to make it public in order to use with xamarin
                        field.Attributes = FieldAttributes.Public;
                        GenerateSelfField(ref selfField, type);
                        UpdateStateMachineMethod(method, field, type, ref selfField);
                    }
                }
            }
        }

        private void UpdateMoveNextMethodNewCompiler(MethodDefinition method, TypeDefinition stateMachineType, ref FieldDefinition selfField)
        {
            method.Body.SimplifyMacros();
            var instructions = method.Body.Instructions;
            VariableDefinition stateMachineAwareVar = null;
            for (var index = 0; index < instructions.Count; index++)
            {
                var line = instructions[index];
                if (line.OpCode != OpCodes.Stfld || line.Previous.OpCode == OpCodes.Ldnull)
                    continue;


                var awaiterField = line.Operand as FieldReference;
                if (awaiterField == null || !awaiterField.Name.StartsWith(Constants.AwaiterNameNew, StringComparison.OrdinalIgnoreCase) ||
                    awaiterField.FieldType.IsValueType)
                    continue;
                GenerateSelfField(ref selfField, stateMachineType);
                //NOTE generate this code
                /*IAsyncStateMachineAware asyncStateMachineAware = this.<>u__$awaiter as IAsyncStateMachineAware;
	              if (asyncStateMachineAware != null && _self_ != null)
	                  asyncStateMachineAware.SetStateMachine(this._self_);*/
                var returnInst = instructions[++index];

                //NOTE generate this code
                /*this._self_ = this;*/
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                if (stateMachineType.IsValueType)
                {
                    instructions.Insert(index++, Instruction.Create(OpCodes.Ldobj, stateMachineType));
                    instructions.Insert(index++, Instruction.Create(OpCodes.Box, stateMachineType));
                }
                instructions.Insert(index++, Instruction.Create(OpCodes.Stfld, selfField));

                if (stateMachineAwareVar == null)
                {
                    stateMachineAwareVar = new VariableDefinition(_asyncStateMachineAwareType);
                    method.Body.Variables.Add(stateMachineAwareVar);
                }
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, awaiterField));
                instructions.Insert(index++, Instruction.Create(OpCodes.Isinst, _asyncStateMachineAwareType));
                instructions.Insert(index++, Instruction.Create(OpCodes.Stloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, selfField));
                instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

                instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, selfField));
                instructions.Insert(index, Instruction.Create(OpCodes.Callvirt, _setStateMachineMethod));
            }

            method.Body.OptimizeMacros();
            LogInfo($"The '{method}' was updated");
        }

        private void UpdateMoveNextMethodOldCompiler(MethodDefinition method, TypeDefinition stateMachineType, ref FieldDefinition selfField)
        {
            method.Body.SimplifyMacros();
            var instructions = method.Body.Instructions;
            VariableDefinition stateMachineAwareVar = null;
            for (var index = 0; index < instructions.Count; index++)
            {
                var line = instructions[index];
                if (line.OpCode != OpCodes.Stfld || line.Previous.OpCode == OpCodes.Ldnull)
                    continue;

                var awaiterField = line.Operand as FieldReference;
                if (awaiterField == null || !awaiterField.Name.Contains(Constants.AwaiterName) ||
                    awaiterField.FieldType.IsValueType)
                    continue;
                GenerateSelfField(ref selfField, stateMachineType);
                //NOTE generate this code
                /*IAsyncStateMachineAware asyncStateMachineAware = this.<>u__$awaiter as IAsyncStateMachineAware;
	              if (asyncStateMachineAware != null && _self_ != null)
	                  asyncStateMachineAware.SetStateMachine(this._self_);*/
                var returnInst = instructions[++index];

                //NOTE generate this this
                /*this._self_ = param0;*/
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                if (stateMachineType.IsValueType)
                {
                    instructions.Insert(index++, Instruction.Create(OpCodes.Ldobj, stateMachineType));
                    instructions.Insert(index++, Instruction.Create(OpCodes.Box, stateMachineType));
                }
                instructions.Insert(index++, Instruction.Create(OpCodes.Stfld, selfField));

                if (stateMachineAwareVar == null)
                {
                    stateMachineAwareVar = new VariableDefinition(_asyncStateMachineAwareType);
                    method.Body.Variables.Add(stateMachineAwareVar);
                }
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, awaiterField));
                instructions.Insert(index++, Instruction.Create(OpCodes.Isinst, _asyncStateMachineAwareType));
                instructions.Insert(index++, Instruction.Create(OpCodes.Stloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, selfField));
                instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

                instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, selfField));
                instructions.Insert(index, Instruction.Create(OpCodes.Callvirt, _setStateMachineMethod));
            }

            method.Body.OptimizeMacros();
            LogInfo($"The '{method}' was updated");
        }

        private void UpdateStateMachineMethod(MethodDefinition method, FieldDefinition field, TypeDefinition stateMachineType, ref FieldDefinition selfField)
        {
            GenerateSelfField(ref selfField, stateMachineType);
            method.Body.SimplifyMacros();
            var stateMachineAwareVar = new VariableDefinition(_asyncStateMachineAwareType);
            method.Body.Variables.Add(stateMachineAwareVar);
            var instructions = method.Body.Instructions;
            var index = instructions.Count - 1;
            var returnInst = instructions[index];

            //NOTE generate this code
            /*this._self_ = param0;*/
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_1));
            instructions.Insert(index++, Instruction.Create(OpCodes.Stfld, selfField));

            //NOTE generate this code
            /*IAsyncStateMachineAware asyncStateMachineAware = this.<>u__$awaiter as IAsyncStateMachineAware;
	        if (asyncStateMachineAware != null)
	            asyncStateMachineAware.SetStateMachine(param0);*/
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldfld, field));
            instructions.Insert(index++, Instruction.Create(OpCodes.Isinst, _asyncStateMachineAwareType));
            instructions.Insert(index++, Instruction.Create(OpCodes.Stloc, stateMachineAwareVar));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
            instructions.Insert(index++, Instruction.Create(OpCodes.Brfalse_S, returnInst));

            instructions.Insert(index++, Instruction.Create(OpCodes.Ldloc, stateMachineAwareVar));
            instructions.Insert(index++, Instruction.Create(OpCodes.Ldarg_1));
            instructions.Insert(index, Instruction.Create(OpCodes.Callvirt, _setStateMachineMethod));

            method.Body.OptimizeMacros();
            LogInfo($"AsyncStateMachine {method.DeclaringType.Name} was updated");
        }

        private static void GenerateSelfField(ref FieldDefinition selfField, TypeDefinition type)
        {
            if (selfField != null)
                return;
            var @interface = type.Interfaces.First(reference => reference.InterfaceType.FullName == Constants.AsyncStateMachineIntefaceFullName);
            selfField = new FieldDefinition("_self_", FieldAttributes.Public, @interface.InterfaceType);
            type.Fields.Add(selfField);
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