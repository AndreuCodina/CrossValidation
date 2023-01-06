﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CrossValidation.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CrossValidation.Resources.ErrorResource", typeof(ErrorResource).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be a valid value.
        /// </summary>
        public static string Enum {
            get {
                return ResourceManager.GetString("Enum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be greater than {FieldValue}.
        /// </summary>
        public static string GreaterThan {
            get {
                return ResourceManager.GetString("GreaterThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must have between {Minimum} and {Maximum} caracter(s).
        /// </summary>
        public static string LengthRange {
            get {
                return ResourceManager.GetString("LengthRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must have at least {Minimum} characters.
        /// </summary>
        public static string MinimumLength {
            get {
                return ResourceManager.GetString("MinimumLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must have a value.
        /// </summary>
        public static string NotNull {
            get {
                return ResourceManager.GetString("NotNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must not have a value.
        /// </summary>
        public static string Null {
            get {
                return ResourceManager.GetString("Null", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The entry is not valid.
        /// </summary>
        public static string Predicate {
            get {
                return ResourceManager.GetString("Predicate", resourceCulture);
            }
        }
    }
}
