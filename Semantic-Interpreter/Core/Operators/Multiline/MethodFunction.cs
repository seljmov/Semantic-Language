﻿using System;

namespace Semantic_Interpreter.Core
{
    public class MethodFunction : BaseFunction, IHaveClassParameter, IHaveReturn
    {
        public MethodFunction() => OperatorId = GenerateOperatorId();

        public string ClassParameter { get; set; }
        public SemanticType SemanticType { get; set; }
        public Return Return { get; set; }
        public sealed override string OperatorId { get; }

        public override void Execute()
        {
            Block.ForEach(x =>
            {
                try
                {
                    x.Execute();
                }
                catch (Exception)
                {
                    throw new  Exception();
                }
            });
            
            VerifyParametersExpressions();
            IHaveBlock.ClearVariableStorage(Block);
        }
    }
}