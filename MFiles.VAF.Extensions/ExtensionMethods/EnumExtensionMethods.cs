// ReSharper disable once CheckNamespace
using System;
using System.Linq;
using MFiles.VAF.Configuration;
using System.Resources;

namespace MFiles.VAF.Extensions
{
	internal static class EnumExtensionMethods
	{
		public static string GetJsonConfEditorHelpText<TEnumType>(this TEnumType enumValue, ResourceManager resourceManager = null)
			where TEnumType : struct
		{
			var enumType = typeof(TEnumType);
			var name = Enum.GetName(enumType, enumValue);
			var jsonConfEditorAttribute = enumType
				.GetField(name)
				.GetCustomAttributes(true)
				.FirstOrDefault(a => a is JsonConfEditorAttribute) as JsonConfEditorAttribute;

			// No label?
			if (string.IsNullOrWhiteSpace(jsonConfEditorAttribute?.HelpText))
				return enumValue.ToString();

			var key = jsonConfEditorAttribute.Label;
			var prefix = jsonConfEditorAttribute.ResourceIdPrefix ?? "$$";
			if (key?.StartsWith(prefix) ?? false)
			{
				// Get the helpText.
				var helpText = resourceManager?.GetString(key.Substring(prefix.Length));
				if (string.IsNullOrWhiteSpace(helpText))
					return enumValue.ToString();
				return helpText;
			}
			return key;
		}
		public static string GetJsonConfEditorLabel<TEnumType>(this TEnumType enumValue, ResourceManager resourceManager = null)
			where TEnumType : struct
		{
			var enumType = typeof(TEnumType);
			var name = Enum.GetName(enumType, enumValue);
			var jsonConfEditorAttribute = enumType
				.GetField(name)
				.GetCustomAttributes(true)
				.FirstOrDefault(a => a is JsonConfEditorAttribute) as JsonConfEditorAttribute;

			// No label?
			if (string.IsNullOrWhiteSpace(jsonConfEditorAttribute?.Label))
				return enumValue.ToString();

			var key = jsonConfEditorAttribute.Label;
			var prefix = jsonConfEditorAttribute.ResourceIdPrefix ?? "$$";
			if (key?.StartsWith(prefix) ?? false)
			{
				// Get the label.
				var label = resourceManager?.GetString(key.Substring(prefix.Length));
				if (string.IsNullOrWhiteSpace(label))
					return enumValue.ToString();
				return label;
			}
			return key;
		}
	}
}
