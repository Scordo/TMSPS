using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class ChatBotPlugin : TMSPSPluginBase
    {
    	#region Constants

    	public const string READ_SETTINGS_RIGHT = "ChatBotReadSettings";
    	public const string READ_SETTINGS_COMMAND = "ReadChatBotSettings";

    	#endregion

    	#region Properties

    	public override Version Version
    	{
    		get { return new Version("1.0.0.0"); }
    	}

    	public override string Author
    	{
    		get { return "Jens Hofmann"; }
    	}

    	public override string Name
    	{
    		get { return "ChatBotPlugin"; }
    	}

    	public override string Description
    	{
    		get { return "Checks for registered phrases and answers to them."; }
    	}

    	public override string ShortNameForLogging
    	{
    		get { return "ChatBotPlugin"; }
    	}

    	private string Botname
    	{
    		get; set;
    	}

    	private Dictionary<string, string> Answers
    	{
    		get; set;
    	}

    	private string[] Phrases
    	{
    		get; set;
    	}

    	#endregion

    	#region Methods

    	protected override void Init()
    	{
    		if (!ReadSettings())
    		{
    			Logger.WarnToUI("ChatBotPlugin not started!");
    			return;
    		}

    		Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
    	}

    	private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
    	{
			RunCatchLog(()=>
			{
    			if (e.Erroneous || e.IsServerMessage || e.Text.IsNullOrTimmedEmpty())
    				return;

    			if (e.IsRegisteredCommand)
    			{
    				CheckForReadSettingsCommand(e);
    				return;
    			}

    			foreach (string phrase in Phrases)
    			{
    				string regexPattern = @"\b" + Regex.Escape(phrase) + @"\b";

    				if (Regex.IsMatch(e.Text, regexPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
    				{
    					Context.RPCClient.Methods.SendServerMessage(string.Format("{0}{1}", Botname, Answers[phrase]));
    					break;
    				}
    			}
			}, "Error in Callbacks_PlayerChat Method.", true);
    	}

    	private bool CheckForReadSettingsCommand(PlayerChatEventArgs e)
    	{
    		if (e.Text.StartsWith("/tmsps ", StringComparison.InvariantCultureIgnoreCase))
    		{
    			string command = e.Text.Substring(7).Trim();

    			if (READ_SETTINGS_COMMAND.Equals(command, StringComparison.InvariantCultureIgnoreCase))
    			{
    				if (Context.Credentials.UserHasRight(e.Login, command))
    				{
    					if (ReadSettings())
    						Context.RPCClient.Methods.SendToLogin(string.Format("{0}ChatBot-Settings successfully read!", Botname), e.Login);
    					else
                            Context.RPCClient.Methods.SendToLogin(string.Format("{0}Error while reading ChatBot-Settings. See log for deailed error description", Botname), e.Login);
    				}
    				else
    				{
                        Context.RPCClient.Methods.SendToLogin(string.Format("{0}You do not have permissions to execute this command!", Botname), e.Login);
    				}

    				return true;
    			}
    		}

    		return false;
    	}

    	protected override void Dispose()
    	{
    		Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
    	}

    	private bool ReadSettings()
    	{
    		string configFilePath = GetConfigFilePath("ChatBot.xml");

    		try
    		{
    			if (!File.Exists(configFilePath))
    				throw new FileNotFoundException("ChatBot.xml not found.", configFilePath);

    			Util.WaitUntilReadable(configFilePath, 10000);
    			XDocument doc = XDocument.Load(configFilePath);

				if (doc.Root == null || doc.Root.Name != "Settings")
    				throw new InvalidOperationException("Could not find settings-root-node in ChatBot.xml.");

    			Botname = "[ChatBot] ";
    			XElement botnameElement = doc.Root.Element("Botname");
    			if (botnameElement != null)
    				Botname = botnameElement.Value;

    			Answers = new Dictionary<string, string>();
    			foreach (XElement phraseElement in doc.Root.Descendants("Phrase"))
    			{
    				XAttribute phraseAttribute = phraseElement.Attribute("values");
    				XAttribute answerAttribute = phraseElement.Attribute("answer");

    				if (answerAttribute == null || phraseAttribute == null)
    					continue;

    				string answer = answerAttribute.Value.Trim();

    				if (answer.IsNullOrTimmedEmpty())
    					continue;

    				string[] phrases = phraseAttribute.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

    				foreach (string phrase in phrases)
    				{
    					if (phrase.IsNullOrTimmedEmpty())
    						continue;

    					Answers[phrase.Trim().ToLower()] = answer;
    				}
    			}

    			Phrases = Answers.Keys.OfType<string>().ToList().ToArray();
    			return true;
    		}
    		catch (Exception ex)
    		{
    			Logger.WarnToUI("Could not read Chatbot.xml! See Error-Log for detailed error description!", ex);
    			Logger.Warn("The following exception occured during reading Chatbot.xml", ex);

    			return false;
    		}
    	}

    	#endregion
    }
}