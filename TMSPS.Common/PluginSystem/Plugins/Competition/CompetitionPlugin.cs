using System;
using System.Collections.Generic;
using System.Text;
using TMSPS.Core.Common;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.Competition
{
    public class CompetitionPlugin : TMSPSPlugin
    {
        #region Properties

    	public override Version Version { get { return new Version("1.0.0.0"); } }
    	public override string Author { get { return "Jens Hofmann"; } }
    	public override string Name { get { return "CompetitionPlugin"; } }
    	public override string Description { get { return "Plugin for starting and joining small competition between players."; } }
    	public override string ShortName { get { return "Competition"; } }
        private CompetitionList RunningCompetitions { get; set; }

    	#endregion

        #region Constructor

        protected CompetitionPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

    	#region Methods

    	protected override void Init()
    	{
            RunningCompetitions = new CompetitionList();

            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
    	}

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            foreach (Competition competition in RunningCompetitions.GetStartedCompetitions())
            {
                competition.UpdateWithEndRaceResult(e.Rankings);
            }

            foreach (Competition competition in RunningCompetitions.GetFinishedCompetitions().ToList())
            {
                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition is over here are the rankings:\n{[Rankings]}", "Rankings", GetRankingText(competition));
                RunningCompetitions.Remove(competition);
            }

            foreach (Competition competition in RunningCompetitions.GetStartedCompetitions())
            {
                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition's current ranking:\n{[Rankings]}", "Rankings", GetRankingText(competition));
            }
        }

        private void Callbacks_PlayerDisconnect(object sender, Communication.EventArguments.Callbacks.PlayerDisconnectEventArgs e)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(e.Login);

            if (competition == null)
                return;

            if (string.Compare(competition.Leader, e.Login, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (competition.Competitors.Count > 1)
                    SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).Where(l => l != e.Login).ToArray(), "{[#ServerStyle]}>{[#ErrorStyle]} The leader of the competition left the game competition is stopped.");    

                RunningCompetitions.Remove(competition);
                return;
            }

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).Where(l => l != e.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} ${[Nickname]}$z{[#MessageStyle]} left the competition, because of leaving the server.", "Nickname", GetNickname(e.Login, true));
        }

        private void Callbacks_PlayerChat(object sender, Communication.EventArguments.Callbacks.PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                ServerCommand command = ServerCommand.Parse(e.Text);

                if (command != null)
                    HandleCommand(e.Login, command);
            }, "Errror in Callbacks_PlayerChat", true);
        }

        private static string GetRankingText(Competition competition)
        {
            StringBuilder result = new StringBuilder();

            int currentRank = 1;

            foreach (Competitor competitor in competition.GetRanking())
            {
                result.AppendFormat("$z{0}. {1}$z --> Score: {2}\n", currentRank, GetNickname(competitor.Login, true), competitor.Score);
                currentRank++;
            }

            return result.ToString();
        }

        private void HandleCommand(string login, ServerCommand command)
        {
            if (command.Is(Command.CreateBattle))
            {
                HandleCreateBattleCommand(login, command);
                return;
            }

            if (command.Is(Command.StartBattle))
            {
                HandleStartBattleCommand(login, command);
                return;
            }

            if (command.Is(Command.JoinBattle))
            {
                HandleJoinBattleCommand(login, command);
                return;
            }

            if (command.Is(Command.LeaveBattle))
            {
                HandleLeaveBattleCommand(login, command);
                return;
            }

            if (command.Is(Command.StopBattle))
            {
                HandleStopBattleCommand(login, command);
                return;
            }
        }

        private void HandleCreateBattleCommand(string login, ServerCommand command)
        {
            if (RunningCompetitions.ContainsCompetitor(login))
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You are already taken part in a competition.");
                return;
            }

            int roundLimit;
            if (!(command.HasFurtherParts && int.TryParse(command.PartsWithoutMainCommand[0], out roundLimit)))
                roundLimit = 5;

            if (roundLimit <= 0)
                roundLimit = 5;

            Competition competition = new Competition(login, roundLimit);
            RunningCompetitions.Add(competition);
            SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Competition with name {[#HighlightStyle]}{[CompetitionName]}{[#MessageStyle]} which lasts {[#HighlightStyle]}{[Rounds]}{[#MessageStyle]} rounds was created.", "CompetitionName", competition.Name, "Rounds", roundLimit.ToString());
        }

        private void HandleStartBattleCommand(string login, ServerCommand command)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not the leader of any competition.");
                return;
            }

            if (string.Compare(competition.Leader, login, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not the leader of the competition your taken part.");
                return;
            }

            if (competition.Competitors.Count < 2)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} No other player is taken part in your competition.");
                return;
            }

            if (competition.Started)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} Competition is already running.");
                return;
            }

            competition.Start();

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition was started!");
        }

        private void HandleJoinBattleCommand(string login, ServerCommand command)
        {
            if (!command.HasFurtherParts)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} Please specify the name of the competition to join.");
                return;
            }

            string competitionName = command.PartsWithoutMainCommand[0];
            Competition competition = RunningCompetitions.GetByName(competitionName);
            
            if (competition == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} There is no competition with name '{[CompetitionName]}'.", "CompetitionName", competitionName);
                return;
            }

            if (competition.IsTakenPart(login))
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're already taken part in this competition.");
                return;
            }

            if (competition.Started)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} This competition is already running. You can not join a running competition.");
                return;
            }

            competition.Join(login);

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} joined the competition.", "Nickname", GetNickname(login, true));
        }

        private void HandleLeaveBattleCommand(string login, ServerCommand command)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not taking part in a competition.");
                return;
            }

            if (string.Compare(competition.Leader, login, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not allowed to leave the competition as a leader. Use {[#HighlightStyle]}/StopBattle {[#ErrorStyle]}to stop the battle.");
                return;
            }

            competition.Leave(login);
            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} ${[Nickname]}$z{[#MessageStyle]} left the competition.", "Nickname", GetNickname(login, true));
        }

        private void HandleStopBattleCommand(string login, ServerCommand command)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null || string.Compare(competition.Leader, login, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not the leader of the competition your taken part. So you cant stop the competition.");
                return;
            }

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} ${[Nickname]}$z{[#MessageStyle]} has stopped the competition.", "Nickname", GetNickname(login, true));

            if (competition.Started && competition.Competitors.Count > 1)
                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition is over here are the rankings:\n{[Rankings]}", "Rankings", GetRankingText(competition));

            RunningCompetitions.Remove(competition);
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.CreateBattle, "Creates a competition lasting X rounds.", "/CreateBattle <RoundLimit>", "/CreateBattle 5"),
                    new CommandHelp(Command.JoinBattle, "Joins a battle with the specified name. When creating a battle the login of the creator is used as the name of the competition.", "/JoinBattle <CompetitionName>", "/JoinBattle scordo"),
                    new CommandHelp(Command.LeaveBattle, "This command is used to leave the competition you're currently taking part in.", "/LeaveBattle", "/LeaveBattle"),
                    new CommandHelp(Command.StartBattle, "Starts the created competition. From now on score is counted for rankings. Joining the competition is no longer possible.", "/StartBattle", "/StartBattle"),
                    new CommandHelp(Command.StopBattle, "Stops the current competition.", "/StopBattle", "/StopBattle"),
                };
            }
        }

        #endregion
    }
}
