using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace MLD.Common.Utils;

/// <summary>
/// Caching utility for enum attributes. Only 3 attribute types supported at this time: DisplayAttribute, EnumMemberAttribute, QsKeyAttribute
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TAttribute"></typeparam>
public static class EnumAttributeCache<T, TAttribute> where T : struct, Enum where TAttribute : Attribute
{
    private static readonly Dictionary<T, string> ValueToAttributeList;

    private static readonly Dictionary<string, T> InsensitiveAttributeToValueList;

    private static readonly Func<TAttribute, string> AccessAttributeValue;
    static EnumAttributeCache()
    {
        if (!AttributeValueAccessors.TryGet<TAttribute>(out var func) || func == null)
        {
            throw new NotSupportedException($"Attribute of type {typeof(TAttribute).FullName} is not supported for caching");
        }

        AccessAttributeValue = func;

        var list = typeof(T).GetFields().Where(x => x.IsLiteral).ToList();

        ValueToAttributeList = new Dictionary<T, string>();
        InsensitiveAttributeToValueList = new Dictionary<string, T>();

        foreach (var fieldInfo in list)
        {
            var attribute = fieldInfo.GetCustomAttribute<TAttribute>(false);
            if (attribute == null)
            {
                continue;
            }

            var attrVal = AccessAttributeValue(attribute);
            if (!string.IsNullOrWhiteSpace(attrVal))
            {
                var item = (T)Enum.Parse(typeof(T), fieldInfo.Name);
                ValueToAttributeList.Add(item, attrVal);
                if (!InsensitiveAttributeToValueList.ContainsKey(attrVal.ToLowerInvariant()))
                {
                    InsensitiveAttributeToValueList.Add(attrVal.ToLower(), item);
                }

            }

        }
    }

    public static string? GetAttributeValue(T val)
    {
        if (ValueToAttributeList.TryGetValue(val, out var attrVal))
        {
            return attrVal;
        }
        return default;
    }

    public static bool TryGetValue(string attributeValue, out T val)
    {
        if (attributeValue == null)
        {
            throw new ArgumentNullException(nameof(attributeValue));
        }
        return InsensitiveAttributeToValueList.TryGetValue(attributeValue.ToLowerInvariant(), out val);
    }

    public static T GetValueDef(string? attributeValue, T defaultValue)
    {
        return !string.IsNullOrEmpty(attributeValue) && InsensitiveAttributeToValueList.TryGetValue(attributeValue.ToLowerInvariant(), out var val)
         ? val
         : defaultValue;
    }

    public static IEnumerable<string> AllAttributeValues => ValueToAttributeList.Values;
}

internal static class AttributeValueAccessors
{
    private static readonly Dictionary<Type, Delegate> PropertyAccessors;

    static AttributeValueAccessors()
    {
        PropertyAccessors = new Dictionary<Type, Delegate>();

        Add<EnumMemberAttribute>(x => x.Value);
        Add<DisplayAttribute>(x => x.Name);
    }

    public static bool TryGet<T>(out Func<T, string>? func) where T : Attribute
    {
        if (PropertyAccessors.TryGetValue(typeof(T), out var f))
        {
            func = f as Func<T, string>;
            return func != null;
        }
        func = null;
        return false;
    }

    private static void Add<T>(Func<T, string> func) where T : Attribute
    {
        PropertyAccessors.Add(typeof(T), func);
    }
}