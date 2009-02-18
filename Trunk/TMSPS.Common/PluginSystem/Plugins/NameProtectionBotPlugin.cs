﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ResponseHandling;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class NameProtectionBotPlugin : TMSPSPlugin
    {
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
            get { return "NameProtectionBotPlugin"; }
    	}

    	public override string Description
    	{
    		get { return "Checks for registered clan members and kicks every player missusing the clantag."; }
    	}

    	public override string ShortName
    	{
            get { return "NameProtectionBot"; }
    	}

    	private HashSet<string> ClanMembers
    	{
    		get; set;
    	}

    	private string Pattern
    	{
    		get; set;
    	}

    	private string KickReason
    	{
    		get; set;
    	}

    	private string PublicKickReason
    	{
    		get; set;
    	}

    	#endregion

    	#region Methods

    	protected override void Init()
    	{
    		ReadConfigValues();
    		Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
    	}

    	private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
    	{
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerConnect] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

			RunCatchLog(()=>
			{
    			if (!ClanMembers.Contains(e.Login.ToLower()))
    			{
    			    PlayerInfo playerInfo = GetPlayerInfoCached(e.Login);

    				if (playerInfo != null)
    				{
    					if (!Regex.IsMatch(playerInfo.NickName, Pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled))
    						return;

    					GenericResponse<bool> kickResponse = Context.RPCClient.Methods.Kick(playerInfo.Login, KickReason);

    					if (kickResponse == null || kickResponse.Value)
    					{
    						Logger.InfoToUI(string.Format("Login {0} with player name {1} was kicked due to name abuse!", e.Login, playerInfo.NickName));
    						Context.RPCClient.Methods.ChatSendServerMessage(string.Format(PublicKickReason, playerInfo.NickName));
    					}
    				}
    			}
			}, "Error in Callbacks_PlayerConnect Method.", true);
    	}

        protected override void Dispose(bool connectionLost)
    	{
    		Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
    	}

    	private void ReadConfigValues()
    	{
            if (!File.Exists(PluginSettingsFilePath))
    		{
    			const string message = "SaveClanTag.xml not found.";
                FileNotFoundException ex = new FileNotFoundException(message, PluginSettingsFilePath);
    			Logger.FatalToUI("SaveClanTag.xml not found.", ex);
    			throw ex;
    		}

            Util.WaitUntilReadable(PluginSettingsFilePath, 10000);
            XDocument doc = XDocument.Load(PluginSettingsFilePath);

    		if (doc.Root == null || doc.Root.Name != "Settings")
    		{
    			const string message = "Could not find settings-root-node in SaveClanTag.xml.";
				ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		XAttribute patternAttribute = doc.Root.Attribute("clanTagPattern");
    		if (patternAttribute == null)
    		{
    			const string message = "Could not find 'clanTagPattern' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		Pattern = patternAttribute.Value.Trim();

    		if (Pattern.IsNullOrTimmedEmpty())
    		{
    			const string message = "Please provide a non empty 'clanTagPattern' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		XAttribute reasonAttribute = doc.Root.Attribute("kickReason");
    		if (reasonAttribute == null)
    		{
    			const string message = "Could not find 'kickReason' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		KickReason = reasonAttribute.Value.Trim();

    		if (KickReason.IsNullOrTimmedEmpty())
    		{
    			const string message = "Please provide a non empty 'kickReason' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		XAttribute publicReasonAttribute = doc.Root.Attribute("publicKickReason");
    		if (publicReasonAttribute == null)
    		{
    			const string message = "Could not find 'publicKickReason' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		PublicKickReason = publicReasonAttribute.Value.Trim();

    		if (PublicKickReason.IsNullOrTimmedEmpty())
    		{
    			const string message = "Please provide a non empty 'publicKickReason' Attribute in Settings-node.";
    			ConfigurationErrorsException ex = new ConfigurationErrorsException(message);
    			Logger.FatalToUI(message, ex);
    			throw ex;
    		}

    		ClanMembers = new HashSet<string>();
    		foreach (XElement loginElement in doc.Root.Descendants("Login"))
    		{
    			if (loginElement.Value.IsNullOrTimmedEmpty())
    				continue;

    			string login = loginElement.Value.Trim();
    			if (!ClanMembers.Contains(login))
    				ClanMembers.Add(login);
    		}
    	}

    	#endregion
    }
}