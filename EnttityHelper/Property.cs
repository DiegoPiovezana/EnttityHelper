using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// Gets or sets the name of the 
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the 
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the value of the propert considering database standard.
        /// </summary>
        public object? ValueSql { get; set; }

        /// <summary>
        /// Gets or sets the type of the 
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
        /// Gets or sets the maximum length of the 
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of the 
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the column name in the database.
        /// </summary>
        public string? ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the regular expression associated with the 
        /// </summary>
        public string? Regex { get; set; }

        /// <summary>
        /// Gets or sets the maximum value allowed for the 
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// Gets or sets the minimum value allowed for the 
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the display of the 
        /// </summary>
        public string? Display { get; set; }

        /// <summary>
        /// Gets or sets the data type of the 
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the 
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the default value for the 
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Indicates whether the property should be ignored during serialization (e.g., for APIs).
        /// </summary>
        public bool? JsonIgnore { get; set; }

        /// <summary>
        /// Gets or sets the error message associated with the 
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error type associated with the 
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error value associated with the 
        /// </summary>
        public string? ErrorValue { get; set; }

        /// <summary>
        /// Gets or sets the error length associated with the 
        /// </summary>
        public string? ErrorLength { get; set; }


        public Property(PropertyInfo propertyInfo, object? objectEntity)
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));

            Name = propertyInfo.Name;
            ColumnName = propertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name;
            IsPrimaryKey = propertyInfo.GetCustomAttribute<KeyAttribute>() != null;
            IsForeignKey = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>() != null;
            IsNotMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;
            IsVirtual = propertyInfo.GetGetMethod()?.IsVirtual ?? false;
            IsRequired = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;

            MaxLength = propertyInfo.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            MinLength = propertyInfo.GetCustomAttribute<MinLengthAttribute>()?.Length;
            MaxLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength;
            MinLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MinimumLength;

            Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            IsNullable = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;

            Value = objectEntity != null ? propertyInfo.GetValue(objectEntity) : null;
            ValueSql = ConvertValueToSqlFormat(Value, propertyInfo.PropertyType);

            static object? ConvertValueToSqlFormat(object? value, Type propertyType)
            {
                return value != null ? propertyType switch
                {
                    Type t when t == typeof(DateTime) => ((DateTime)value).ToString(),
                    Type t when t == typeof(decimal) => ((decimal)value).ToString(),
                    Type t when t == typeof(bool) => (bool)value ? 1 : 0,
                    _ => value
                } : null;
            }
        }



        override public string? ToString()
        {
            return ValueSql?.ToString();
        }


    }



}
