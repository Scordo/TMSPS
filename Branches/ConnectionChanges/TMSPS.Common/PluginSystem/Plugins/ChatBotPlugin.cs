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
    public class ChatBotPlugin : TMSPSPlugin
    {
    	#region Properties

    	public override Version Version { get { return new Version("1.0.0.0"); } }
    	public override string Author { get { return "Jens Hofmann"; } }
    	public override string Name { get { return "ChatBotPlugin"; } }
    	public override string Description { get { return "Checks for registered phrases and answers to them."; } }
    	public override string ShortName { get { return "ChatBot"; } }
    	private string Botname { get; set; }
        private Dictionary<string, ChatBotAnswer> Answers { get; set; }
    	private string[] Phrases { get; set; }

    	#endregion

        #region Constructor

        protected ChatBotPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
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

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.ReadChatBotSettings, "Refreshes the in memory settings of the chat bot by reading from settings.xml.", "/t readchatbotsettings", "/t readchatbotsettings"),
                };
            }
        }

    	private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
    	{
			RunCatchLog(()=>
			{
    			if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty())
    				return;

                if (e.IsRegisteredCommand && CheckForReadSettingsCommand(e))
    				return;

                if (e.IsRegisteredCommand)
                    return;

    			foreach (string phrase in Phrases)
    			{
    				string regexPattern = @"\b" + Regex.Escape(phrase) + @"\b";

    				if (!Regex.IsMatch(e.Text, regexPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
    				    continue;

    			    ChatBotAnswer answer = Answers[phrase];

                    if (answer.Wisper)
                        SendFormattedMessageToLogin(e.Login, string.Format("{0}{1}", Botname, answer.Answer));
                    else
                        SendFormattedMessage(string.Format("{0}{1}", Botname, answer.Answer));
    				
                    break;
    			}
			}, "Error in Callbacks_PlayerChat Method.", true);
    	}

    	private bool CheckForReadSettingsCommand(PlayerChatEventArgs e)
    	{
            if (ServerCommand.Parse(e.Text).Is(Command.ReadChatBotSettings))
    		{
                if (!LoginHasRight(e.Login, true, Command.ReadChatBotSettings))
                    return true;
				
                if (ReadSettings())
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}ChatBot-Settings successfully read!");
				else
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#ErrorStyle]}Error while reading ChatBot-Settings. See log for detailed error description.");

				return true;
    		}

    		return false;
    	}

    	private bool ReadSettings()
    	{
    		try
    		{
    			if (!File.Exists(PluginSettingsFilePath))
                    throw new FileNotFoundException("ChatBot.xml not found.", PluginSettingsFilePath);

                Util.WaitUntilReadable(PluginSettingsFilePath, 10000);
                XDocument doc = XDocument.Load(PluginSettingsFilePath);

				if (doc.Root == null || doc.Root.Name != "Settings")
    				throw new InvalidOperationException("Could not find settings-root-node in ChatBot.xml.");

    			Botname = "[ChatBot] ";
    			XElement botnameElement = doc.Root.Element("Botname");
    			if (botnameElement != null)
    				Botname = botnameElement.Value;

                Answers = new Dictionary<string, ChatBotAnswer>();
    			foreach (XElement phraseElement in doc.Root.Descendants("Phrase"))
    			{
    				XAttribute phraseAttribute = phraseElement.Attribute("values");
    				XAttribute answerAttribute = phraseElement.Attribute("answer");
                    XAttribute wisperAttribute = phraseElement.Attribute("wisper");

    				if (answerAttribute == null || phraseAttribute == null)
    					continue;

    				string answer = answerAttribute.Value.Trim();
    			    bool wisper = (wisperAttribute != null) && (string.Compare(wisperAttribute.Value, "true", true) == 0);

    				if (answer.IsNullOrTimmedEmpty())
    					continue;

    				string[] phrases = phraseAttribute.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

    				foreach (string phrase in phrases)
    				{
    					if (phrase.IsNullOrTimmedEmpty())
    						continue;

                        Answers[phrase.Trim().ToLower()] = new ChatBotAnswer(answer, wisper);
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

        #region Embedded Types

        private class ChatBotAnswer
        {
            #region Properties

            public string Answer { get; set; }
            public bool Wisper { get; set; }

            #endregion

            #region Constructors

            public ChatBotAnswer()
            {
                
            }

            public ChatBotAnswer(string answer, bool wisper)
            {
                Answer = answer;
                Wisper = wisper;
            }

            #endregion
        }

        #endregion

    }
}