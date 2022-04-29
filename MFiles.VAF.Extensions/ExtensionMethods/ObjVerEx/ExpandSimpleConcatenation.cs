﻿using MFiles.VAF.Common;
using MFilesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// The regular expression used to extract property matches ("%PROPERTY_0%").
		/// </summary>
		public static readonly Regex ExtractPlaceholders = new Regex(
			"(?<replacementText>(?<prefix>\\%)(?<type>[a-zA-Z]*?ID)?(?<type>OBJECTURL_[a-zA-Z]*?)?(?<reference>(?<type>PROPERTY)_(\\{?(?<aliasorid>[^%]+?)\\}?)?\\.?)*(?<suffix>\\%))",
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

			// Use the internal implementation.
			return objVerEx.ExpandSimpleConcatenation
			(
				concatenationString,
				(o, id) => o.GetPropertyText(id),
				(o, id) => o.GetDirectReference(id),
				(o, platform) => UrlHelper.GetObjectUrl(o, platform)
			);

		}

		/// <summary>
		/// Expands a simple string using similar logic to the "concatenated properties"
		/// functionality in M-Files Admin.
		/// More lightweight than <see cref="ObjVerEx.ExpandPlaceholderText(string, bool, bool)"/> by only supporting
		/// simple property expressions (%PROPERTY_123%, %PROPERTY_{Alias}%, %PROPERTY_{Alias1}.PROPERTY_{Alias2}%, %EXTERNALID% and %INTERNALID%).
		/// </summary>
		/// <param name="objVerEx">The object containing information to use for the expansion.</param>
		/// <param name="concatenationString">The string containing the placeholders.</param>
		/// <returns>The expanded string.</returns>
		internal static string ExpandSimpleConcatenation
		(
			this ObjVerEx objVerEx,
			string concatenationString,
			Func<ObjVerEx, int, string> getPropertyText,
			Func<ObjVerEx, int, ObjVerEx> getDirectReference,
			Func<ObjVerEx, UrlTargetPlatform, string> getObjectUrl
		)
		{	
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (string.IsNullOrWhiteSpace(concatenationString))
				return string.Empty;
			if (null == getPropertyText)
				throw new ArgumentNullException(nameof(getPropertyText));
			if (null == getDirectReference)
				throw new ArgumentNullException(nameof(getDirectReference));
			if (null == getObjectUrl)
				throw new ArgumentNullException(nameof(getObjectUrl));

			// Try and get all placeholders.
			return ExtractPlaceholders.Replace(concatenationString, (Match match) =>
			{
				// Sanity.
				if(!match.Success)
					return match.Value;

				// What is the type?
				switch(match.Groups["type"]?.Value?.ToLower())
				{
					case "displayid":
						return (objVerEx.Info?.DisplayIDAvailable ?? false)
						? objVerEx.Info.DisplayID // DisplayID if we have it.
						: objVerEx.Info?.OriginalObjID != null
						? objVerEx.Info.OriginalObjID.ID.ToString() // Original ID if we have it.
						: objVerEx.ID.ToString(); // Internal ID otherwise.
					case "objecttypeid":
						return objVerEx.Type.ToString();
					case "objectversionid":
						return objVerEx.Version.ToString();
					case "objectguid":
						return objVerEx.Info?.ObjectGUID;
					case "vaultguid":
						return objVerEx.Vault?.GetGUID();
					case "internalid":
						return objVerEx.ID.ToString();
					case "externalid":
						return objVerEx.Info?.DisplayID;
					case "property":
					{
						// Extract the vault structural references.
						var aliasOrIds = match.Groups["aliasorid"];

						// No type, die.
						if(!aliasOrIds.Success)
							return match.Value;

						// Get the property ID references.
						var propertyIds = aliasOrIds.ToPropertyIds(objVerEx.Vault);

						// Iterate over the properties to build up the string.
						var host = objVerEx;
						for(var i =0; i<propertyIds.Count; i++)
						{
							// Get the property ID.
							var propertyId = propertyIds[i];

							// Not found or invalid value?
							if (propertyId < 0)
								throw new ArgumentException
								(
									string.Format
									(
										Resources.Exceptions.ObjVerExExtensionMethods.ExpandSimpleConcatenation_PropertyNotFound,
										aliasOrIds.Captures[i],
										aliasOrIds.Value
									),
									nameof(concatenationString)
								);

							// If we are on the last one then get the property value.
							if (i == propertyIds.Count - 1)
								return getPropertyText(host, propertyId);

							// Otherwise, alter the host.
							host = getDirectReference(host, propertyId);

							// Sanity.
							if (null == host)
								return string.Empty;
						}

						// No replacement.
						return match.Value;

					}
					case "objecturl_web":
						return getObjectUrl(objVerEx, UrlTargetPlatform.Web);
					case "objecturl_desktop":
						return getObjectUrl(objVerEx, UrlTargetPlatform.Desktop);
					case "objecturl_mobile":
						return getObjectUrl(objVerEx, UrlTargetPlatform.Mobile);
					default:
						return match.Value;
				}
			});

		}

		/// <summary>
		/// Converts property definition aliases or IDs found in a <paramref name="group"/>'s captures into
		/// property definition IDs from the provided vault.
		/// </summary>
		/// <param name="group">The group to extract captures from.</param>
		/// <param name="vault">The vault to look aliases up in.</param>
		/// <returns>A set of property ids.</returns>
		internal static IList<int> ToPropertyIds(this Group group, Vault vault)
		{
			// Sanity.
			if (null == group)
				throw new ArgumentNullException(nameof(group));
			if (null == vault)
				throw new ArgumentNullException(nameof(vault));

			// Output for the properties.
			var output = new List<int>(group.Captures.Count);

			// Iterate over the references to build up the string.
			for(var i =0; i<group.Captures.Count; i++)
			{
				// Get the vault structural reference string.
				var propertyReference = group.Captures[i].Value;

				// If it is not an integer then treat it as an alias.
				if (!Int32.TryParse(propertyReference, out int propertyId))
				{
					// It's an alias - resolve it to an id.
					propertyId = vault
						.PropertyDefOperations
						.GetPropertyDefIDByAlias(propertyReference);
				}

				// Add the resolved property ID to the collection.
				output.Add(propertyId < 0 ? -1 : propertyId);
			}

			// Return the collection.
			return output;
		}
	}
}
