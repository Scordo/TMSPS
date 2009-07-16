using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Configuration;
using TMSPS.Core.Common;
using System;

namespace TMSPS.Core.PluginSystem
{
    public class MessageStyles : ReadOnlyDictionary<string, string>
    {
        #region Constructor

        private MessageStyles(IDictionary<string, string> source) : base(source)
        {
            
        }

        #endregion

        #region Public Methods

        public static MessageStyles ReadFromFileOrGetDefault(string xmlConfigurationFile)
        {
            return File.Exists(xmlConfigurationFile) ? ReadFromFile(xmlConfigurationFile) : new MessageStyles(new Dictionary<string, string>());
        }

        public static MessageStyles ReadFromFile(string xmlConfigurationFile)
        {
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach(XElement node in configDocument.Root.Nodes())
            {
                result[node.Name.ToString().ToLower()] = node.Value;
            }

            return new MessageStyles(result);
        }

        public string ReplaceStyles(string text)
        {
            if (text.IsNullOrTimmedEmpty())
                return text;

            if (Count == 0)
                return text;

            return Regex.Replace(text, @"{\[#(?<tag>[^\[{#}\]]+)\]}", match =>
            {
                string tag = match.Groups["tag"].Value.ToLower(CultureInfo.InvariantCulture);
                return ContainsKey(tag) ? this[tag] : string.Empty;
            }, RegexOptions.Singleline | RegexOptions.Compiled);
        }

        #endregion
    }
}