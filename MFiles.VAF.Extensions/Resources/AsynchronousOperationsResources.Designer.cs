﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MFiles.VAF.Extensions.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AsynchronousOperationsResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AsynchronousOperationsResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MFiles.VAF.Extensions.Resources.AsynchronousOperationsResources", typeof(AsynchronousOperationsResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No timespan specified; does not repeat..
        /// </summary>
        internal static string RepeatType_Interval_NoTimeSpanSpecified {
            get {
                return ResourceManager.GetString("RepeatType_Interval_NoTimeSpanSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Runs on demand (does not repeat)..
        /// </summary>
        internal static string RepeatType_RunsOnDemandOnly {
            get {
                return ResourceManager.GetString("RepeatType_RunsOnDemandOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unhandled repeat type: {0}.
        /// </summary>
        internal static string RepeatType_UnhandledRepeatType {
            get {
                return ResourceManager.GetString("RepeatType_UnhandledRepeatType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Running.
        /// </summary>
        internal static string Status_Running {
            get {
                return ResourceManager.GetString("Status_Running", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scheduled.
        /// </summary>
        internal static string Status_Scheduled {
            get {
                return ResourceManager.GetString("Status_Scheduled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopped.
        /// </summary>
        internal static string Status_Stopped {
            get {
                return ResourceManager.GetString("Status_Stopped", resourceCulture);
            }
        }
    }
}
