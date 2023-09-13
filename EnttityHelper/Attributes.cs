using System;
using System.Collections.Generic;
using System.Text;

namespace EH
{
    internal class Attributes
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        internal sealed class CustomDescriptionAttribute : Attribute
        {
            public string Description { get; }

            public CustomDescriptionAttribute(string description)
            {
                Description = description;
            }
        }

        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        internal sealed class MyAttribute : Attribute
        {
            public string Message { get; }

            public MyAttribute(string message)
            {
                Message = message;
            }
        }


    }
}
