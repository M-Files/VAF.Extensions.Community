using MFiles.VAF.Common;
using System;
using System.Text.RegularExpressions;

namespace MFiles.VAF.Extensions.ExtensionMethods
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// The regular expression used to extract property matches ("%PROPERTY_0%").
		/// </summary>
		private static Regex extractPlaceholders = new Regex(
			"(?<replacementText>(?<prefix>\\%)(?<reference>(?<type>PROPERTY_)(\\{?(?<aliasorid>[^%]+?)\\}?)?\\.?)+(?<suffix>\\%))",
			RegexOptions.Multiline
			| RegexOptions.ExplicitCapture
			| RegexOptions.CultureInvariant
			| RegexOptions.Compiled
		);

		/// <summary>
		/// Expands a simple string using similar logic to the "concatenated properties"
		/// functionality in M-Files Admin.
		/// More lightweight than <see cref="ObjVerEx.ExpandPlaceholderText(string, bool, bool)"/> by only supporting
		/// simple property expressions (%PROPERTY_123%, %PROPERTY_{Alias}%, %PROPERTY_{Alias1}.PROPERTY_{Alias2}%, %EXTERNALID% and %INTERNALID%).
		/// </summary>
		/// <param name="objVerEx">The object containing information to use for the expansion.</param>
		/// <param name="concatenationString">The string containing the placeholders.</param>
		/// <returns>The expanded string.</returns>
		public static string ExpandSimpleConcatenation
		(
			this ObjVerEx objVerEx,
			string concatenationString
		)
		{	
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (string.IsNullOrWhiteSpace(concatenationString))
				return string.Empty;

			// Replace simple strings.
			concatenationString = concatenationString.Replace("%INTERNALID%", objVerEx.ID.ToString());
			concatenationString = concatenationString.Replace("%EXTERNALID%", objVerEx.Info.DisplayID);

			// Try and get all property placeholders.
			return extractPlaceholders.Replace(concatenationString, (Match match) =>
			{
				// Sanity.
				if(!match.Success)
					return match.Value;

				// Extract the vault structural references.
				var aliasOrIds = match.Groups["aliasorid"];

				// No type, die.
				if(!aliasOrIds.Success)
					return match.Value;

				// Iterate over the references to build up the string.
				var host = objVerEx;
				for(var i =0; i<aliasOrIds.Captures.Count; i++)
				{
					// Get the vault structural reference string.
					var propertyReference = aliasOrIds.Captures[i].Value;

					// If it's an integer then it's a property id.
					if (Int32.TryParse(propertyReference, out int propertyId))
						return objVerEx.GetPropertyText(propertyId);

					// It's an alias - resolve it to an id.
					propertyId = objVerEx
						.Vault
						.PropertyDefOperations
						.GetPropertyDefIDByAlias(propertyReference);

					// Not found == empty string.
					if (-1 == propertyId)
						return string.Empty;

					// If we are on the last one then get the property value.
					if (i == aliasOrIds.Captures.Count - 1)
						return host.GetPropertyText(propertyId);

					// Otherwise, alter the host.
					host = host.GetDirectReference(propertyId);
				}

				// No replacement.
				return match.Value;
			});

		}
	}
}
