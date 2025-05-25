using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace EH.Properties
{
    /// <summary>
    /// Represents a property with metadata and additional attributes, enabling advanced
    /// property handling, mapping, and metadata storage for use in database operations
    /// or object property reflection.
    /// </summary>
    public class Property
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
        /// Gets or sets the value of the property considering the database standard. Example: Bool to 1/0.
        /// </summary>
        public object? ValueText { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        public Type? Type { get; set; }

        /// <summary>
        /// Gets or sets the database type of the property.
        /// </summary>
        public DbType DbType { get; set; }

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
        /// The property primary key.
        /// </summary>
        public KeyAttribute? PrimaryKey { get; set; }

        /// <summary>
        /// The property foreign key.
        /// </summary>
        public ForeignKeyAttribute? ForeignKey { get; set; }

        /// <summary>
        /// The property inverse property.
        /// </summary>
        public InversePropertyAttribute? InverseProperty { get; set; }

        /// <summary>
        /// Indicates whether the property is a collection.
        /// </summary>
        public bool? IsCollection { get; set; }

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

        // /// <summary>
        // /// Gets or sets the regular expression associated with the 
        // /// </summary>
        // public string? Regex { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the maximum value allowed for the property.
        // /// </summary>
        // public int? Max { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the minimum value allowed for the property.
        // /// </summary>
        // public int? Min { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the display of the property.
        // /// </summary>
        // public string? Display { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the data type of the property.
        // /// </summary>
        // public string? DataType { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the display name of the property.
        // /// </summary>
        // public string? DisplayName { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the default value for the property.
        // /// </summary>
        // public object? DefaultValue { get; set; }
        //
        // /// <summary>
        // /// Indicates whether the property should be ignored during serialization (e.g., for APIs).
        // /// </summary>
        // public bool? JsonIgnore { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the error message associated with the property.
        // /// </summary>
        // public string? ErrorMessage { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the error type associated with the property.
        // /// </summary>
        // public string? ErrorType { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the error value associated with the property.
        // /// </summary>
        // public string? ErrorValue { get; set; }
        //
        // /// <summary>
        // /// Gets or sets the error length associated with the property.
        // /// </summary>
        // public string? ErrorLength { get; set; }


        public Property(PropertyInfo propertyInfo, object? objectEntity)
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));

            Name = propertyInfo.Name;
            ColumnName = propertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name;
            PrimaryKey = propertyInfo.GetCustomAttribute<KeyAttribute>();
            ForeignKey = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>();
            InverseProperty = propertyInfo.GetCustomAttribute<InversePropertyAttribute>();

            IsNotMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;
            IsVirtual = propertyInfo.GetGetMethod()?.IsVirtual ?? false;
            IsRequired = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;
            IsCollection = propertyInfo.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

            MaxLength = propertyInfo.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            MinLength = propertyInfo.GetCustomAttribute<MinLengthAttribute>()?.Length;
            MaxLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength;
            MinLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MinimumLength;

            Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            DbType = ToolsProp.MapToDbType(Type);
            IsNullable = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;

            Value = objectEntity != null ? propertyInfo.GetValue(objectEntity) : null;
            ValueText = ConvertValueToSqlTextFormat(Value, propertyInfo.PropertyType);
            
            static object? ConvertValueToSqlTextFormat(object? value, Type propertyType)
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
            return ValueText?.ToString();
        }
    }
}
