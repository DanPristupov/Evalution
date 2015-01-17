namespace Evalution
{
    using System;

    public class InvalidNameException : Exception
    {
        public InvalidNameException(string name)
            : base(string.Format("Cannot find name '{0}' in the current context.", name))
        {
        }
    }
}