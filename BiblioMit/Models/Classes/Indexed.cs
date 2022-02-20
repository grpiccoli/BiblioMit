using BiblioMit.Services;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace BiblioMit.Models
{
    public class Indexed : IHasBasicIndexer
    {
        [ParseSkip]
        public object? this[string propertyName]
        {
            get
            {
                PropertyInfo? propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo == null) return null;
                return propertyInfo.GetValue(this, null);
            }
            set
            {
                PropertyInfo? propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                    if (value is null)
                    {
                        if (propertyInfo.PropertyType.IsGenericType)
                        {
                            if (propertyInfo.PropertyType.IsClass && propertyInfo.CanWrite)
                            {
                                SetValue(this, propertyInfo, propertyName, value);
                            }
                            else
                            {
                                Type def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                                if (propertyInfo.GetSetMethod() == null && def == typeof(ICollection<>))
                                {
                                    object? list = propertyInfo.GetValue(this, null);
                                    propertyInfo.PropertyType.InvokeMember("Clear",
                                        BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                        null, list, null,
                                        CultureInfo.InvariantCulture);
                                }
                                else if (def == typeof(Nullable<>) && propertyInfo.CanWrite)
                                {
                                    SetValue(this, propertyInfo, propertyName, value);
                                }
                                else
                                {
                                    throw new InvalidCastException();
                                }
                            }
                        }
                    }
                    else
                    {
                        Type inputType = value.GetType();
                        if (inputType.IsGenericType)
                        {
                            Type def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                            if (propertyInfo.GetSetMethod() == null && def == typeof(ICollection<>))
                            {
                                object? list = propertyInfo.GetValue(this, null);
                                propertyInfo.PropertyType.InvokeMember("Clear",
                                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                    null, list, null,
                                    CultureInfo.InvariantCulture);
                                foreach (object item in (ICollection)value)
                                    propertyInfo.PropertyType.InvokeMember("Add",
                                        BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                        null, list, new object[] { item },
                                        CultureInfo.InvariantCulture);
                            }
                        }
                        else if (inputType == propertyInfo.PropertyType && propertyInfo.CanWrite)
                        {
                            SetValue(this, propertyInfo, propertyName, value);
                        }
                        else
                        {
                            if (propertyInfo.PropertyType.IsGenericType)
                            {
                                Type def = propertyInfo.PropertyType.GetGenericTypeDefinition();
                                if (def == typeof(Nullable<>) && propertyInfo.CanWrite)
                                {
                                    Type? t = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                                    if (inputType == t)
                                        SetValue(this, propertyInfo, propertyName, value);
                                }
                            }
                            else
                            {
                                TypeConverter typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                                object? propValue = typeConverter.ConvertFromString(value?.ToString() ?? string.Empty);
                                SetValue(this, propertyInfo, propertyName, propValue);
                            }
                        }
                    }
            }
        }
        public static void SetValue(Indexed entity, PropertyInfo propertyInfo, string propertyName, object? value)
        {
            //if (propertyInfo == null) return;
            MethodInfo? method = propertyInfo.GetSetMethod(true);
            if (method == null) return;
            if (method.IsPublic)
                propertyInfo.SetValue(entity, value, null);
            else
                propertyInfo.PropertyType.InvokeMember($"Set{propertyName}",
                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                null, entity, new object?[] { value },
                CultureInfo.InvariantCulture);
        }
    }
}
