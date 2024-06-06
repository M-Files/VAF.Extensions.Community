using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	public static class TimeZoneInformationExtensionMethods
	{
		private static readonly ILogger Logger
			= LogManager.GetLogger(typeof(TimeZoneInformationExtensionMethods));

		/// <summary>
		/// Ensures that <paramref name="input"/> is in local time.  This is important when
		/// searching, as searches expect the value to be in local time.
		/// account the time zone offset for <see cref="SessionInfo.TimeZoneInfo"/>.
		/// </summary>
		/// <param name="timeZoneInfo">The time zone for the vault connection.</param>
		/// <param name="input"></param>
		/// <returns>
		/// If <paramref name="timeZoneInfo"/> is null, or contains an unknown <see cref="TimeZoneInformation.StandardName"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is already set to <see cref="DateTimeKind.Local"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is set to <see cref="DateTimeKind.Unspecified"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is set to <see cref="DateTimeKind.Utc"/> then the datetime is altered to take into the <paramref name="timeZoneInfo"/>.
		/// </returns>
		public static DateTime EnsureLocalTime(this TimeZoneInformation timeZoneInfo, DateTime input)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(timeZoneInfo?.StandardName))
				return input;

			// If we don't know what kind it is then die.
			if (input.Kind == DateTimeKind.Unspecified)
			{
				Logger?.Warn
				(
					$"Could not identify whether the DateTime represents a local or UTC time.  It will be used as-is."
				);
				return input;
			}

			// If we are already in local time then die.
			if (input.Kind == DateTimeKind.Local)
				return input;

			// Adjust by the offset.
			try
			{
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo.StandardName);
				var output = input.Add(tzi.GetUtcOffset(input));
				return DateTime.SpecifyKind(output, DateTimeKind.Local);
			}
			catch (Exception ex)
			{
				Logger?.Error
				(
					ex,
					$"Error converting to local time.  It will be used as-is."
				);
				return input;
			}
		}

		/// <summary>
		/// Ensures that <paramref name="input"/> is in UTC time.  This is important when setting
		/// property values, as these values are expected to be in UTC.
		/// account the time zone offset for <see cref="SessionInfo.TimeZoneInfo"/>.
		/// </summary>
		/// <param name="timeZoneInfo">The time zone for the vault connection.</param>
		/// <param name="input"></param>
		/// <returns>
		/// If <paramref name="timeZoneInfo"/> is null, or contains an unknown <see cref="TimeZoneInformation.StandardName"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is already set to <see cref="DateTimeKind.Local"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is set to <see cref="DateTimeKind.Unspecified"/> then this method does nothing.
		/// If <see cref="DateTime.Kind"/> is set to <see cref="DateTimeKind.Utc"/> then the datetime is altered to take into the <paramref name="timeZoneInfo"/>.
		/// </returns>
		public static DateTime EnsureUTCTime(this TimeZoneInformation timeZoneInfo, DateTime input)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(timeZoneInfo?.StandardName))
				return input;

			// If we don't know what kind it is then die.
			if (input.Kind == DateTimeKind.Unspecified)
			{
				Logger?.Warn
				(
					$"Could not identify whether the DateTime represents a local or UTC time.  It will be used as-is."
				);
				return input;
			}

			// If we are already in UTC time then die.
			if (input.Kind == DateTimeKind.Utc)
				return input;

			// Adjust by the offset.
			try
			{
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo.StandardName);
				var output = input.Subtract(tzi.GetUtcOffset(input));
				return DateTime.SpecifyKind(output, DateTimeKind.Utc);
			}
			catch (Exception ex)
			{
				Logger?.Error
				(
					ex,
					$"Error converting to UTC time.  It will be used as-is."
				);
				return input;
			}
		}
	}
}
