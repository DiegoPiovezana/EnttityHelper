using System;
using System.Reflection;

namespace EH
{
    internal class Property
    {
        /// <summary>
        /// Gets or sets the PropertyInfo.
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the value of the propert considering database standard.
        /// </summary>
        public object? ValueSql { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        public Type? Type { get; set; }
              
        /// <summary>
        /// Indicates whether the property is nullable.
        /// </summary>
        public bool? IsNullable { get; set; }

        /// <summary>
        /// Indicates whether the property is required.
        /// </summary>
        public bool? IsRequired { get; set; }

        /// <summary>
        /// Indicates whether the property is virtual.
        /// </summary>
        public bool? IsVirtual { get; set; }

        /// <summary>
        /// Indicates whether the property is not mapped.
        /// </summary>
        public bool? IsNotMapped { get; set; }

        /// <summary>
        /// Indicates whether the property is a primary key.
        /// </summary>
        public bool? IsPrimaryKey { get; set; }

        /// <summary>
        /// Indicates whether the property is a foreign key.
        /// </summary>
        public bool? IsForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the property.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of the property.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the column name in the database.
        /// </summary>
        public string? ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the regular expression associated with the property.
        /// </summary>
        public string? Regex { get; set; }

        /// <summary>
        /// Gets or sets the maximum value allowed for the property.
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// Gets or sets the minimum value allowed for the property.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the display of the property.
        /// </summary>
        public string? Display { get; set; }

        /// <summary>
        /// Gets or sets the data type of the property.
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the property.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the default value for the property.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Indicates whether the property should be ignored during serialization (e.g., for APIs).
        /// </summary>
        public bool? JsonIgnore { get; set; }

        /// <summary>
        /// Gets or sets the error message associated with the property.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error type associated with the property.
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error value associated with the property.
        /// </summary>
        public string? ErrorValue { get; set; }

        /// <summary>
        /// Gets or sets the error length associated with the property.
        /// </summary>
        public string? ErrorLength { get; set; }


        public Property(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        override public string? ToString()
        {
            return ValueSql?.ToString();
        }


    }

    

}
