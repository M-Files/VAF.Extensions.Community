using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/>
		/// is entering <paramref name="state"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="state">The target state.</param>
		/// <returns><see langword="true"/> if entering the state, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, MFIdentifier state)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (null == state)
				return false;

			// Make sure we resolve it if we need to.
			if (!state.IsResolved)
			{
				state.Resolve(objVerEx.Vault, typeof(MFilesAPI.State), forceRefresh: false);
				if (!state.IsResolved)
					return false;
			}

			// See whether it is entering the given state.
			return objVerEx.IsEnteringState(state.ID);
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/>
		/// is entering <paramref name="state"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="state">The target state.</param>
		/// <returns><see langword="true"/> if entering the state, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, int state)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			if (state <= 0)
				return false;

			// See whether it is entering the given state.
			// Note: The "IsEnteringState" property will load a previous version (heavy), so short-circuit if the state ID doesn't match.
			return objVerEx.State == state
				&& objVerEx.IsEnteringState;
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/> is entering
		/// one of the provided <paramref name="states"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="state">The target states.</param>
		/// <returns><see langword="true"/> if entering one of the states, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, IEnumerable<MFIdentifier> states)
			=> objVerEx.IsEnteringState(states, out _);

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/> is entering
		/// one of the provided <paramref name="states"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="states">The target states.</param>
		/// <param name="stateBeingEntered">The state being entered.  Null if not entering one of the given <paramref name="states"/>.</param>
		/// <returns><see langword="true"/> if entering one of the states, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, IEnumerable<MFIdentifier> states, out MFIdentifier stateBeingEntered)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			stateBeingEntered = null;
			if (null == states || false == states.Any())
				return false;

			// Check each state in turn.
			foreach (var state in states)
			{
				if (!objVerEx.IsEnteringState(state))
					continue;

				// Entering this state.
				stateBeingEntered = state;
				return true;
			}

			// Nope.
			return false;
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/> is entering
		/// one of the provided <paramref name="states"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="states">The target states.</param>
		/// <returns><see langword="true"/> if entering one of the states, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, IEnumerable<int> states)
			=> objVerEx.IsEnteringState(states, out _);

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="objVerEx"/> is entering
		/// one of the provided <paramref name="states"/>.
		/// </summary>
		/// <param name="objVerEx">The object to check.</param>
		/// <param name="states">The target states.</param>
		/// <param name="stateBeingEntered">The state being entered.  -1 if not entering one of the given <paramref name="states"/>.</param>
		/// <returns><see langword="true"/> if entering one of the states, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="objVerEx"/> is null.</exception>
		public static bool IsEnteringState(this ObjVerEx objVerEx, IEnumerable<int> states, out int stateBeingEntered)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));
			stateBeingEntered = -1;
			if (null == states || false == states.Any())
				return false;

			// Check each state in turn.
			foreach (var state in states)
			{
				if (!objVerEx.IsEnteringState(state))
					continue;

				// Entering this state.
				stateBeingEntered = state;
				return true;
			}

			// Nope.
			return false;
		}
	}
}
