﻿using Semantic_Interpreter.Library;

namespace Semantic_Interpreter.Core
{
    public class Module : MultilineOperator
    {
        public Module(string name)
        {
            Name = name;
            OperatorId = GenerateOperatorId();
        }
        
        public readonly VariableStorage VariableStorage = new();
        public readonly FunctionStorage FunctionStorage = new();
        public readonly ClassStorage ClassStorage = new();
        
        public string Name { get; set; }
        public override string OperatorId { get; }

        public override void Execute() {}
    }
}