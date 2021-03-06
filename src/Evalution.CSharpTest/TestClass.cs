﻿using System;

namespace Evalution.Tests
{
    public class ClassInt32
    {
        public int Value1 { get; set; }

        public virtual int ValueWithExpression { get; set; }

        public virtual int DependentValue1 { get; set; }

        public virtual int DependentValue2 { get; set; }

        public virtual int DependentValue3 { get; set; }

        public virtual int DependentValue4 { get; set; }

        public int Multiply(int value1, int value2)
        {
            return value1*value2;
        }
    }

    public class ClassDouble
    {
        public double Value1 { get; set; }

        public virtual double ValueWithExpression { get; set; }

        public virtual double DependentValue1 { get; set; }

        public virtual double DependentValue2 { get; set; }
    }

    public class ClassDateTime
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }

        public virtual TimeSpan ValueWithExpression1 { get; set; }

        public virtual TimeSpan ValueWithExpression2 { get; set; }

        public virtual DateTime DependentValue1 { get; set; }

        public virtual DateTime DependentValue2 { get; set; }

        public virtual TimeSpan DependentValue3 { get; set; }
    }

    public class ClassArray
    {
        public int[] IntValues { get; set; }
        public ClassDateTime[] ComplexObjects { get; set; }

        public virtual int DependentValue1 { get; set; }

        public virtual int DependentValue2 { get; set; }

        public virtual TimeSpan DependentValue3 { get; set; }

        public virtual DateTime DependentValue4 { get; set; }
    }

    public class ComplexObject
    {
        public ClassInt32 Child { get; set; }

        public ComplexObject ComplexChild { get; set; }

        public virtual int DependentValue1 { get; set; }

        public virtual int DependentValue2 { get; set; }

        public int Multiply(int value1, int value2)
        {
            return value1*value2;
        }
    }

    public class ClassWithConstructor
    {
        public ClassWithConstructor()
        {
            this.DefaultContstructorCalled = true;
        }

        public ClassWithConstructor(int param)
        {
            this.IntConstructurCalled = true;
        }

        public ClassWithConstructor(int param1, int param2)
        {
            this.DoubleIntConstructurCalled = true;
        }

        public bool DoubleIntConstructurCalled { get; set; }

        public bool IntConstructurCalled { get; set; }

        public bool DefaultContstructorCalled { get; set; }
    }
}