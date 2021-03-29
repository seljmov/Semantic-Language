﻿using Semantic_Interpreter.Library;
using Semantic_Interpreter.Parser.Operators;

namespace Semantic_Interpreter.Parser.Expressions
{
    public class VariableExpression : IExpression, IOperator
    {
        public VariableExpression(SemanticTypes type, string name, IExpression expression)
        {
            Type = type;
            Name = name;
            Expression = expression;
        }

        public SemanticTypes Type { get; set; }
        public string Name { get; set; }
        public IExpression Expression { get; set; }

        public IValue Eval()
        {
            // TODO: Create vars storage
            throw new System.NotImplementedException();
        }

        public override string ToString() => string.Format(Name);
        public void Execute() { }
    }
}