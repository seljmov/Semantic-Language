﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Semantic_Interpreter.Core;
using Semantic_Interpreter.Library;

namespace Semantic_Interpreter.Parser
{
    public class Parser
    {
        private static readonly Token Eof = new(TokenType.Eof, "");

        private readonly List<Token> _tokens;
        private readonly int _length;

        private int _pos;
        private readonly SemanticTree _semanticTree = new();
        
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _length = tokens.Count;
            _pos = 0;
        }

        public SemanticTree Parse()
        {
            Stack<SemanticOperator> operatorsStack = new();
            SemanticOperator lastOperator = null;
            
            while (!Match(TokenType.Eof))
            {
                if (Match(TokenType.End)) return _semanticTree;

                var prevOperator = lastOperator;
                var newOperator = ParseOperator();
                var asChild = false;
                switch (newOperator)
                {
                    case Module:
                    {
                        operatorsStack.Push(newOperator);
                        break;
                    }
                    
                    case Beginning:
                    {
                        asChild = operatorsStack.Pop().Child == null;
                        operatorsStack.Push(newOperator);
                        break;
                    }
                    
                    case While:
                    {
                        asChild = operatorsStack.Peek().Child == null;
                        break;
                    }
                    
                    case Variable:
                    {
                        asChild = operatorsStack.Peek().Child == null;
                        break;
                    }
                    
                    case Let:
                    {
                        asChild = operatorsStack.Peek().Child == null; 
                        break;
                    }
                    
                    case Input:
                    {
                        asChild = operatorsStack.Peek().Child == null;
                        break;
                    }
                    
                    case Output:
                    {
                        asChild = operatorsStack.Peek().Child == null;
                        break;
                    }
                }
                
                _semanticTree.InsertOperator(prevOperator, newOperator, asChild);
                lastOperator = newOperator;
            }

            return _semanticTree;
        }

        private SemanticOperator ParseOperator()
        {
            if (Match(TokenType.Module))
            {
                return ModuleOperator();
            }
            
            if (Match(TokenType.Beginning))
            {
                return new Beginning();
            }
            
            if (Match(TokenType.While))
            {
                var expression = ParseExpression();
                Consume(TokenType.Word); // Skip then
                var block = new BlockSemanticOperator();
                while (!Match(TokenType.End))
                {
                    block.Add(ParseOperator());
                }

                Consume(TokenType.While);
                Consume(TokenType.Dot);
                return new While(expression, block);
            }
            
            if (Match(TokenType.Variable))
            {
                return VariableOperator();
            }

            if (Match(TokenType.Let))
            {
                var variable = Consume(TokenType.Word).Text;
                Consume(TokenType.Assing);
                var expression = ParseExpression();
                Consume(TokenType.Semicolon);
                
                return new Let(variable, expression);
            }
            
            if (Match(TokenType.Input))
            {
                var name = Consume(TokenType.Word).Text;
                Consume(TokenType.Semicolon);

                return new Input(name);
            }
            
            if (Match(TokenType.Output))
            {
                var expression = ParseExpression();
                Consume(TokenType.Semicolon);

                return new Output(expression);
            }

            return null;
        }
        
        private SemanticOperator ModuleOperator()
        {
            var name = Consume(TokenType.Word).Text;
            return new Module(name);
        }

        private SemanticOperator VariableOperator()
        {
            Consume(TokenType.Minus);  // Skip -
            var type = Consume(TokenType.Word).Text switch
            {
                "integer" => SemanticTypes.Integer,
                "real" => SemanticTypes.Real,
                "boolean" => SemanticTypes.Boolean,
                _ => SemanticTypes.String
            };
            var name = Get().Text;
            IExpression expression = null;
            if (Match(TokenType.Word) && Get().Type == TokenType.Assing)
            {
                Consume(TokenType.Assing);
                expression = ParseExpression();
            }
            var variable = new Variable(type, name, expression);
            VariablesStorage.Add(name, variable);
            Consume(TokenType.Semicolon);
            return variable;
        }
        
        
        private IExpression ParseExpression()
        {
            return LogicalOr();
        }

        private IExpression LogicalOr()
        {
            var result = LogicalAnd();

            while (true)
            {
                if (Match(TokenType.OrOr))
                {
                    result = new ConditionalExpression(TokenType.OrOr, result, LogicalAnd());
                    continue;
                }
                break;
            }

            return result;
        }

        private IExpression LogicalAnd()
        {
            var result = Equality();

            while (true)
            {
                if (Match(TokenType.AndAnd))
                {
                    result = new ConditionalExpression(TokenType.AndAnd, result, Equality());
                    continue;
                }
                
                break;
            }

            return result;
        }

        private IExpression Equality()
        {
            var result = Conditional();

            if (Match(TokenType.Equal))
            {
                return new ConditionalExpression(TokenType.Equal, result, Conditional());
            }

            if (Match(TokenType.NotEqual))
            {
                return new ConditionalExpression(TokenType.NotEqual, result, Conditional());
            }

            return result;
        }

        private IExpression Conditional()
        {
            var result = Additive();

            while (true)
            {
                if (Match(TokenType.Less))
                {
                    result = new ConditionalExpression(TokenType.Less, result, Additive());
                    continue;
                }

                if (Match(TokenType.LessOrEqual))
                {
                    result = new ConditionalExpression(TokenType.LessOrEqual, result, Additive());
                    continue;
                }

                if (Match(TokenType.Greater))
                {
                    result = new ConditionalExpression(TokenType.Greater, result, Additive());
                    continue;
                }

                if (Match(TokenType.GreaterOrEqual))
                {
                    result = new ConditionalExpression(TokenType.GreaterOrEqual, result, Additive());
                    continue;
                }
                
                break;
            }

            return result;
        }

        private IExpression Additive()
        {
            var result = Multiplicative();
            
            while (true)
            {
                if (Match(TokenType.Plus))
                {
                    result = new BinaryExpression(Operations.Plus, result, Multiplicative());
                    continue;
                }

                if (Match(TokenType.Minus))
                {
                    result = new BinaryExpression(Operations.Minus, result, Multiplicative());
                    continue;
                }
                
                break;
            }

            return result;
        }

        private IExpression Multiplicative()
        {
            var result = Unary();

            while (true)
            {
                if (Match(TokenType.Multiply))
                {
                    result = new BinaryExpression(Operations.Multiply, result, Unary());
                    continue;
                }

                if (Match(TokenType.Divide))
                {
                    result = new BinaryExpression(Operations.Divide, result, Unary());
                    continue;
                }
                
                break;
            }

            return result;
        }

        private IExpression Unary()
        {
            return Match(TokenType.Minus) 
                ? new UnaryExpression(Operations.Minus, Primary()) 
                : Primary();
        }
        
        private IExpression Primary()
        {
            var current = Get();
            if (Match(TokenType.Number))
            {
                // Если точки нет, то число целое, иначе - вещественное
                if (!current.Text.Contains('.'))
                    return new ValueExpression(Convert.ToInt32(current.Text));
                
                IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
                return new ValueExpression(Convert.ToDouble(current.Text, formatter));
            }

            if (Match(TokenType.Word))
                return VariablesStorage.At(current.Text);
            
            if (Match(TokenType.Text))
                return new ValueExpression(current.Text);

            if (Match(TokenType.LParen))
            {
                var result = ParseExpression();
                Match(TokenType.RParen);
                return result;
            }

            throw new Exception("Неизвестный оператор.");
        }
        
        private Token Consume(TokenType type)
        {
            var current = Get();
            if (type != current.Type) 
                throw new Exception($"Токен '{current}' не найден ({type}).");
            
            _pos++;
            return current;
        }

        private bool Match(TokenType type)
        {
            var current = Get();
            if (type != current.Type) 
                return false;
            
            _pos++;
            return true;
        }

        private Token Get(int i = 0)
        {
            var position = _pos + i;
            return position >= _length 
                ? Eof 
                : _tokens[position];
        }
    }
}