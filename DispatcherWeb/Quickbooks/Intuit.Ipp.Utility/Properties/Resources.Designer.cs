﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intuit.Ipp.Utility.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Intuit.Ipp.Utility.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to API response without error code element..
        /// </summary>
        internal static string ErrorCodeMissing {
            get {
                return ResourceManager.GetString("ErrorCodeMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error code &quot;{0}&quot; not numeric!.
        /// </summary>
        internal static string ErrorCodeNonNemeric {
            get {
                return ResourceManager.GetString("ErrorCodeNonNemeric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} (Error {1}, Detail: {2}).
        /// </summary>
        internal static string ErrorDetails0 {
            get {
                return ResourceManager.GetString("ErrorDetails0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} (Error {1}).
        /// </summary>
        internal static string ErrorDetails1 {
            get {
                return ResourceManager.GetString("ErrorDetails1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error {0}.
        /// </summary>
        internal static string ErrorWithNoText {
            get {
                return ResourceManager.GetString("ErrorWithNoText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must provide id or name of the field.
        /// </summary>
        internal static string MustProvideIdOrName {
            get {
                return ResourceManager.GetString("MustProvideIdOrName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to n/a.
        /// </summary>
        internal static string NotAvailable {
            get {
                return ResourceManager.GetString("NotAvailable", resourceCulture);
            }
        }
    }
}
