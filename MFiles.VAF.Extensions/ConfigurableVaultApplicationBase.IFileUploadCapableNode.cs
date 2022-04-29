using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.AdminConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
		: IFileUploadCapableNode
	{
		private readonly Dictionary<string, Action<IConfigurationRequestContext, ClientOperations, FileUpload>> dashboardFileUploadHandlerRegistrations
			 = new Dictionary<string, Action<IConfigurationRequestContext, ClientOperations, FileUpload>>();

		/// <summary>
		/// Registers <paramref name="handler"/> to be executed if an upload with ID <paramref name="uploadId"/> is encountered.
		/// </summary>
		/// <param name="uploadId">The ID of the upload to handle.</param>
		/// <param name="handler">The handler of the upload.</param>
		/// <param name="throwIfAnotherHandlerRegistered">If false, silently overwrites any existing handler registrations.</param>
		/// <exception cref="InvalidOperationException">If <paramref name="throwIfAnotherHandlerRegistered"/> is true, and another handler is already registered for this <paramref name="uploadId"/>.</exception>
		protected void RegisterDashboardFileUploadHandler
		(
			string uploadId,
			Action<IConfigurationRequestContext, ClientOperations, FileUpload> handler,
			bool throwIfAnotherHandlerRegistered = true
		)
		{
			// Sanity.
			if (string.IsNullOrWhiteSpace(uploadId))
				throw new ArgumentException("Missing upload ID.", nameof(uploadId));
			if(null == handler)
				throw new ArgumentNullException(nameof(handler));

			lock (this._lock)
			{
				// Handle there already being a registration for this upload id.
				if (this.dashboardFileUploadHandlerRegistrations.ContainsKey(uploadId))
				{
					if (throwIfAnotherHandlerRegistered)
						throw new InvalidOperationException($"Cannot register second upload handler for ID {uploadId}");
					this.RemoveDashboardFileUploadHandlerRegistration(uploadId);
				}

				// Add our handler.
				dashboardFileUploadHandlerRegistrations.Add(uploadId, handler);
			}
		}

		/// <summary>
		/// Retrieves the upload IDs of any existing upload handler registrations.
		/// </summary>
		/// <returns>The IDs.</returns>
		protected IEnumerable<string> GetDashboardFileUploadHandlerRegistrations()
			=> this.dashboardFileUploadHandlerRegistrations.Keys;

		/// <summary>
		/// Removes any handlers already registered for uploads with ID <paramref name="uploadId"/>.
		/// </summary>
		/// <param name="uploadId">The ID of the upload.</param>
		/// <remarks>Does not throw if <paramref name="uploadId"/> has not been registered.</remarks>
		protected void RemoveDashboardFileUploadHandlerRegistration(string uploadId)
		{
			lock (this._lock)
			{
				// Remove if it exists.
				if (this.dashboardFileUploadHandlerRegistrations.ContainsKey(uploadId))
					this.dashboardFileUploadHandlerRegistrations.Remove(uploadId);
			}
		}

		/// <summary>
		/// Handles any file uploads.
		/// </summary>
		/// <param name="context">Contains vault and user info.</param>
		/// <param name="clientOps">Allows updating MFAdmin in response to the upload.</param>
		/// <param name="upload"></param>
		public virtual void HandleFileUpload
		(
			IConfigurationRequestContext context,
			ClientOperations clientOps,
			FileUpload upload
		)
		{
			lock (this._lock)
			{
				// If we have a registration then fire it now.
				if (this.dashboardFileUploadHandlerRegistrations.ContainsKey(upload.Id))
					this.dashboardFileUploadHandlerRegistrations[upload.Id](context, clientOps, upload);
			}

		}

	}
}
