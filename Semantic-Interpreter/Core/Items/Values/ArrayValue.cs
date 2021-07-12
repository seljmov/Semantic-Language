﻿using System;
using System.Reflection;

namespace Semantic_Interpreter.Core
{
    public class ArrayValue : IValue, ICloneable
    {
        public ArrayValue(int size)
        {
            if (size <= 0)
            {
                throw new Exception("Длина массива не может быть меньше 1");
            }

            Size = size;
            Values = new IValue[size];
        }

        public ArrayValue(IValue[] values)
        {
            Values ??= new IValue[values.Length];
            Array.Copy(values, Values, values.Length);
            Size = values.Length;
        }

        public int Size { get; }
        private IValue[] Values { get; }

        public IValue Get(int index) => Values[index];

        public void Set(int index, IValue value) => Values[index] = value;

        public int AsInteger()
            => throw new Exception("Невозможно преобразовать массив к целому числу");

        public double AsReal()
            => throw new Exception("Невозможно преобразовать массив к вещественному числу");

        public bool AsBoolean() => Values.Length != 0;

        public char AsChar()
            => throw new Exception("Невозможно преобразовать массив к символу");

        public string AsString() => Values.ToString();

        public IValue[] AsArray() => Values;

        public override string ToString() => AsString();
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}