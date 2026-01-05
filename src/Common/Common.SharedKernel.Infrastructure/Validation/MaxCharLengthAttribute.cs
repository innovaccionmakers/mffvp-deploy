using System;

namespace Common.SharedKernel.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MaxCharLengthAttribute : Attribute
    {
        public int MaxLength { get; }

        public MaxCharLengthAttribute(int maxLength) => MaxLength = maxLength;
    }
}