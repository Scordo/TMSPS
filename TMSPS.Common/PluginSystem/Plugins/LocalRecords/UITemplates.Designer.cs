﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TMSPS.Core.PluginSystem.Plugins.LocalRecords.UITemplates", typeof(UITemplates).Assembly);
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
        ///   Looks up a localized string similar to &lt;manialink id=&quot;{[ManiaLinkID]}&quot;&gt;
        ///	&lt;frame posn=&quot;53.5 -35.2 0&quot;&gt;
        ///		&lt;quad sizen=&quot;10.5 2&quot;/&gt;
        ///		&lt;format textsize=&quot;2&quot;/&gt;
        ///		&lt;label posn=&quot;4.1 0.0 2&quot; halign=&quot;right&quot; text=&quot;Local:&quot;/&gt;
        ///		&lt;label posn=&quot;4.6 0.0 2&quot; halign=&quot;left&quot; text=&quot;{[LCL]}&quot; /&gt;
        ///	&lt;/frame&gt;             
        ///&lt;/manialink&gt;.
        /// </summary>
        internal static string LowerRightLocalRecordPanelActive {
            get {
                return ResourceManager.GetString("LowerRightLocalRecordPanelActive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;manialink id=&quot;{[ManiaLinkID]}&quot;&gt;&lt;/manialink&gt;.
        /// </summary>
        internal static string LowerRightLocalRecordPanelInactive {
            get {
                return ResourceManager.GetString("LowerRightLocalRecordPanelInactive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;manialink id=&quot;{[ManiaLinkID]}&quot;&gt;
        ///	&lt;frame posn=&quot;53.5 -33 0&quot;&gt;
        ///		&lt;quad sizen=&quot;10.5 2.2&quot;/&gt;
        ///		&lt;format textsize=&quot;2&quot;/&gt;
        ///		&lt;label posn=&quot;4.1 0.0 2&quot; halign=&quot;right&quot; text=&quot;PB:&quot;/&gt;
        ///		&lt;label posn=&quot;4.6 0.0 2&quot; halign=&quot;left&quot; text=&quot;{[PB]}&quot; /&gt;
        ///	&lt;/frame&gt;             
        ///&lt;/manialink&gt;.
        /// </summary>
        internal static string LowerRightPBPanelActive {
            get {
                return ResourceManager.GetString("LowerRightPBRecordPanelActive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;manialink id=&quot;{[ManiaLinkID]}&quot;&gt;&lt;/manialink&gt;.
        /// </summary>
        internal static string LowerRightPBPanelInactive {
            get {
                return ResourceManager.GetString("LowerRightPBRecordPanelInactive", resourceCulture);
            }
        }
    }
}
