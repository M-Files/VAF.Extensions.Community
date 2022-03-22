using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions.Tests.Dashboards
{
	public class StyleComparisonHelper : Dictionary<string, string>
	{
		public StyleComparisonHelper(string expectedStyle)
			: this(expectedStyle?.Split(";".ToCharArray())
				.Where(s => s.Length > 0)
				.Select(p => p.Split(":".ToCharArray()))
				.ToDictionary
				(
					a => (a[0] ?? "").Trim(),
					a => a.Length > 1
						? (string.Join(":", a.Skip(1)) ?? "").Trim()
						: null
				))
		{
		}
		public StyleComparisonHelper(IEnumerable<KeyValuePair<string, string>> items)
		{
			if (null != items)
				foreach (var x in items)
					this.Add(x.Key, x.Value);
		}
		public bool TestAgainstString(string input)
		{
			// Everything in our dictionary must appear in the input.
			if (this.Count > 0 && string.IsNullOrWhiteSpace(input))
				return false;
			if (this.Count == 0 && string.IsNullOrWhiteSpace(input))
				return true;

			// Split by semi-colon, then colon.
			var dict = input.Split(";".ToCharArray())
				.Where(s => s.Length > 0)
				.Select(p => p.Split(":".ToCharArray()))
				.ToDictionary
				(
					a => (a[0] ?? "").Trim(),
					a => a.Length > 1
						? (string.Join(":", a.Skip(1)) ?? "").Trim()
						: null
				);

			// Make sure they all exist and match.
			foreach (var k in this.Keys)
			{
				if (false == dict.ContainsKey(k))
				{
					Assert.Fail($"Target string does not contain key {k}.");
				}
				if (this[k] != dict[k])
				{
					Assert.Fail($"Value for {k} is not correct (expected:{this[k]}, actual: {dict[k]}).");
					return false;
				}
			}

			return true;
		}
	}
}
