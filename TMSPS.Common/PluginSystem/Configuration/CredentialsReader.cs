using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Configuration
{
	public class CredentialsReader
	{

		public static Dictionary<string, HashSet<string>> ReadCredentials(string filePathOrFileContent, bool isFilePath)
		{
            if (isFilePath && !File.Exists(filePathOrFileContent))
				return new Dictionary<string, HashSet<string>>();

            if (isFilePath)
                Util.WaitUntilReadable(filePathOrFileContent, 10000);

            XDocument settingsFile = isFilePath ? XDocument.Load(filePathOrFileContent) : XDocument.Parse(filePathOrFileContent);

			if (settingsFile.Root == null || settingsFile.Root.Name != "Settings")
				return new Dictionary<string, HashSet<string>>();

			Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();

			foreach (XElement groupElement in settingsFile.Root.Descendants("Group"))
			{
				CredentialsGroup group = CredentialsGroup.GetFromXElement(groupElement);
				if (group == null)
					continue;

				foreach (string username in group.Users)
				{
					if (result.ContainsKey(username))
						result[username].UnionWith(group.Rights);
					else
						result[username] = new HashSet<string>(group.Rights);
				}
			}

			XElement usersElement = settingsFile.Root.Element("Users");

			if (usersElement != null)
			{
				foreach (XElement userElement in usersElement.Elements("User"))
				{
					CredentialsUser user = CredentialsUser.GetFromXElement(userElement);
					if (user == null)
						continue;

					if (result.ContainsKey(user.Name))
						result[user.Name].UnionWith(user.Rights);
					else
						result[user.Name] = new HashSet<string>(user.Rights);
				}
			}

			return result;
		}

		class CredentialsGroup
		{
			public HashSet<string> Rights { get; set;}
			public HashSet<string> Users { get; set; }

			private CredentialsGroup()
			{
				Rights = new HashSet<string>();
				Users = new HashSet<string>();
			}

			public static CredentialsGroup GetFromXElement(XContainer groupElement)
			{
				if (groupElement == null)
					return null;

				List<XElement> userElements = groupElement.Descendants("User").ToList();

				if (userElements.Count == 0)
					return null;

				List<XElement> rightElements = groupElement.Descendants("Right").ToList();

				if (rightElements.Count == 0)
					return null;

				List<string> usernames = userElements.ConvertAll(element => element.Value.Trim());
				List<string> rightnames = rightElements.ConvertAll(element => element.Value.Trim());

				CredentialsGroup result = new CredentialsGroup
				{
					Users = new HashSet<string>(usernames.FindAll(name => name.Trim().Length > 0).ConvertAll(name => name.ToLower())),
					Rights = new HashSet<string>(rightnames.FindAll(name => name.Trim().Length > 0).ConvertAll(name => name.ToLower()))
				};

				return (result.Users.Count == 0 || result.Rights.Count == 0) ? null : result;
			}
		}

		class CredentialsUser
		{
			public string Name { get; set; }
			public HashSet<string> Rights { get; set; }

			public static CredentialsUser GetFromXElement(XContainer groupElement)
			{
				if (groupElement == null)
					return null;

				XElement nameElement = groupElement.Element("Name");

				if (nameElement == null)
					return null;

				string name = nameElement.Value.Trim().ToLower();

				if (name.Length == 0)
					return null;

				List<XElement> rightElements = groupElement.Descendants("Right").ToList();

				if (rightElements.Count == 0)
					return null;

				List<string> rightnames = rightElements.ConvertAll(element => element.Value.Trim());

				CredentialsUser result = new CredentialsUser
				{
					Name = name,
					Rights = new HashSet<string>(rightnames.FindAll(rightname => rightname.Trim().Length > 0).ConvertAll(rightname => rightname.ToLower()))
				};

				return (result.Rights.Count == 0) ? null : result;
			}
		}
	}

	
}
