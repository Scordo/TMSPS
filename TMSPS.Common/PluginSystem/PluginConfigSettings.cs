using System.Configuration;

namespace TMSPS.Core.PluginSystem
{
    public class PluginConfigSettings : ConfigurationElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PluginConfigSettings"/> is deserialized.
        /// </summary>
        /// <value><c>true</c> if deserialized; otherwise, <c>false</c>.</value>
        public bool Deserialized { get; set; }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        [ConfigurationProperty("assemblyName", IsRequired = true)]
        public string AssemblyName
        {
            get { return (string)base["assemblyName"]; }
        }

        /// <summary>
        /// Gets the adapter provider class.
        /// </summary>
        /// <value>The adapter provider class.</value>
        [ConfigurationProperty("pluginClass", IsRequired = true)]
        public string PluginClass
        {
            get { return (string)base["pluginClass"]; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PluginConfigSettings"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)base["enabled"]; }
        }

        #endregion

        #region Non Public Methods

        /// <summary>
        /// Called after deserialization.
        /// </summary>
        protected override void PostDeserialize()
        {
            Deserialized = true;
        }

        #endregion
    }
}