using MLD.Common.Utils;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace MLD.Common.Extensions;

public static class EnumExtensions
{
    public static string? GetEnumMemberValue<T>(this T value)
        where T : Enum
    {
        return typeof(T)
            .GetTypeInfo()
            .DeclaredMembers
            .SingleOrDefault(x => x.Name == value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>(false)
            ?.Value;
    }

    /// <summary>
    /// Yuck. Had to make this because the generic method cannot be called with non-generic type sintax. This is the non-cached way of getting the attribute from an enum.
    /// It's not a big issue though since this is sparsely used in the backend, so not worries about performance, but would be nice to have this polished.
    /// </summary>
    /// <param name="enumItem"></param>
    /// <returns></returns>
    public static string GetLabel(this Enum enumItem)
    {
        var memberInfo = enumItem
            .GetType()
            .GetTypeInfo()
            .DeclaredMembers
            .SingleOrDefault(x => x.Name == enumItem.ToString());

        if (memberInfo == null)
            return enumItem.ToString();

        var result = memberInfo.GetCustomAttribute<DisplayAttribute>()?.Name;
        if (result.HasValue())
        {
            return result;
        }

        result = memberInfo.GetCustomAttribute<EnumMemberAttribute>()?.Value;
        if (result.HasValue())
        {
            return result;
        }

        return enumItem.ToString();
    }

    public static string GetLabel<T>(this T enumItem) where T : struct, Enum
    {

        var result = EnumAttributeCache<T, DisplayAttribute>.GetAttributeValue(enumItem);


        if (result.HasValue())
        {
            return result!;
        }

        result = EnumAttributeCache<T, EnumMemberAttribute>.GetAttributeValue(enumItem);
        if (result.HasValue())
        {
            return result!;
        }

        return enumItem.ToString();
    }

    public static string ToCamelCaseString(this Enum enumItem)
    {
        var input = enumItem.ToString();
        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}