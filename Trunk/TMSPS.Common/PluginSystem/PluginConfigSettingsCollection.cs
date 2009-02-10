﻿using System.Configuration;

namespace TMSPS.Core.PluginSystem
{
    [ConfigurationCollection(typeof(PluginConfigSettings))]
    public class PluginConfigSettingsCollection : ConfigurationElementCollection<PluginConfigSettings>
    {
        #region Non Public Methods

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigSettings)element).PluginClass;
        }

        #endregion
    }
}