﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.21006.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins {
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
    internal class UITemplates {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UITemplates() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TMSPS.Core.PluginSystem.Plugins.PodiumPlugins.UITemplates", typeof(UITemplates).Assembly);
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
        ///   Looks up a localized string similar to &lt;frame posn=&quot;0 {[Y]} 0&quot;&gt;
        ///				&lt;format textsize=&quot;1&quot;/&gt;
        ///				&lt;label sizen=&quot;2.5 2&quot; halign=&quot;right&quot; posn=&quot;2.5 0 0.1&quot; text=&quot;{[Rank]}&quot; textcolor=&quot;FFFF&quot;/&gt;
        ///				&lt;label sizen=&quot;3 2&quot; halign=&quot;right&quot; posn=&quot;5.8 0 0.1&quot; text=&quot;{[Value]}&quot; textcolor=&quot;5AFF&quot;/&gt;
        ///				&lt;label sizen=&quot;10 2&quot; posn=&quot;6.1 0 0.1&quot; text=&quot;{[Description]}&quot; textcolor=&quot;FFFF&quot;/&gt;
        ///			&lt;/frame&gt;.
        /// </summary>
        internal static string EntryTemplate {
            get {
                return ResourceManager.GetString("EntryTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;manialink id=&quot;{[ManiaLinkID]}&quot;&gt;
        ///				&lt;frame posn=&quot;{[X]} 48 0&quot;&gt;
        ///					&lt;format textsize=&quot;1&quot;/&gt;
        ///					&lt;label posn=&quot;8.05 -0.5 2&quot; sizen=&quot;16.1 2&quot; halign=&quot;center&quot; text=&quot;{[Title]}&quot;/&gt;
        ///					&lt;EntryPlaceHolder /&gt;
        ///				&lt;/frame&gt;
        ///			&lt;/manialink&gt;.
        /// </summary>
        internal static string MainTemplate {
            get {
                return ResourceManager.GetString("MainTemplate", resourceCulture);
            }
        }
    }
}
