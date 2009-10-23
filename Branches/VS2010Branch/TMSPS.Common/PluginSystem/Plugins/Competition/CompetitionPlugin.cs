using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition's current ranking after {[DR]} of {[RL]} rounds:\n{[Rankings]}", "DR", competition.DrivenRounds.ToString(), "RL", competition.RoundLimit.ToString(), "Rankings", GetRankingText(competition));
            }
        }

        private void Callbacks_PlayerDisconnect(object sender, Communication.EventArguments.Callbacks.PlayerDisconnectEventArgs e)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(e.Login);

            if (competition == null)
                return;

            if (competition.IsLeader(e.Login))
            {
                if (competition.Competitors.Count <= 1)
                {
                    RunningCompetitions.Remove(competition);
                    return;
                }

                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} left the competition.", "Nickname", GetNickname(e.Login, true));

                if (competition.Competitors.Count == 2)
                {
                    SendFormattedMessageToLogin(competition.Competitors.Where(c => c.Login != competition.Leader).Select(c => c.Login).First(), "{[#ServerStyle]}>{[#MessageStyle]} All competitors have left the competition. Competition was stopped.");
                    RunningCompetitions.Remove(competition);
                    return;
                }

                string newLeader = competition.SelectNewLeader();
                SendFormattedMessageToLogins(competition.Competitors.Select(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z is now the leader of the competition. The new competition name is {[#HighlightStyle]}{[Login]}{[#MessageStyle]}.", "Nickname", GetNickname(newLeader, true), "Login", newLeader);

                return;
            }

            competition.Leave(e.Login);
            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).Where(l => l != e.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} left the competition, because of leaving the server.", "Nickname", GetNickname(e.Login, true));

            if (competition.Competitors.Count == 1)
            {
                SendFormattedMessageToLogin(competition.Leader, "{[#ServerStyle]}>{[#MessageStyle]} All competitors have left the competition. Competition was stopped.");
                RunningCompetitions.Remove(competition);
            }
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

            return result.ToString().TrimEnd();
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
                HandleStartBattleCommand(login);
                return;
            }

            if (command.Is(Command.JoinBattle))
            {
                HandleJoinBattleCommand(login, command);
                return;
            }

            if (command.Is(Command.LeaveBattle))
            {
                HandleLeaveBattleCommand(login);
                return;
            }

            if (command.Is(Command.StopBattle))
            {
                HandleStopBattleCommand(login);
                return;
            }

            if (command.Is(Command.ShowBattles))
            {
                HandleShowBattlesCommand(login);
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

            int roundLimit = 5;
            string password = null;
            if (command.PartsWithoutMainCommand.Count > 0)
            {
                bool isNumber = Regex.IsMatch(command.PartsWithoutMainCommand[0], @"\d+", RegexOptions.Singleline);
                password = command.PartsWithoutMainCommand.Count > 1 ? command.PartsWithoutMainCommand[1] : null;

                if (isNumber)
                {
                    if (!(int.TryParse(command.PartsWithoutMainCommand[0], out roundLimit)))
                        roundLimit = 5;

                    if (roundLimit <= 0)
                        roundLimit = 5;
                }
                else
                {
                    password = command.PartsWithoutMainCommand[0];
                }
            }

            Competition competition = new Competition(login, roundLimit, password);
            RunningCompetitions.Add(competition);

            string passwordSentencePostfix = (password == null) ? string.Empty : " with password {[#HighlightStyle]}" + password;
            SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Competition with name {[#HighlightStyle]}{[CompetitionName]}{[#MessageStyle]} which lasts {[#HighlightStyle]}{[Rounds]}{[#MessageStyle]} rounds was created" + passwordSentencePostfix+ ".", "CompetitionName", competition.Name, "Rounds", roundLimit.ToString());
        }

        private void HandleStartBattleCommand(string login)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not the leader of any competition.");
                return;
            }

            if (!competition.IsLeader(login))
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

            if (competition.IsPasswordProtected)
            {
                if (command.PartsWithoutMainCommand.Count < 2)
                {
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} The competition you want to join is password protected but you did not provide the join-password.");
                    return;
                }

                string joinPassword = command.PartsWithoutMainCommand[1];

                if (string.Compare(competition.Password, joinPassword, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} The password for joining the competition is wrong.");
                    return;
                }
            }

            competition.Join(login);

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} joined the competition.", "Nickname", GetNickname(login, true));
        }

        private void HandleLeaveBattleCommand(string login)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not taking part in a competition.");
                return;
            }

            if (competition.IsLeader(login))
            {
                if (competition.Competitors.Count == 1)
                {
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Competition was stopped.");
                    RunningCompetitions.Remove(competition);
                    return;
                }

                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} left the competition.", "Nickname", GetNickname(login, true));

                if (competition.Competitors.Count == 2)
                {
                    SendFormattedMessageToLogin(competition.Competitors.Where(c => c.Login != competition.Leader).Select(c => c.Login).First(), "{[#ServerStyle]}>{[#MessageStyle]} All competitors have left the competition. Competition was stopped.");
                    RunningCompetitions.Remove(competition);
                    return;
                }

                string newLeader = competition.SelectNewLeader();

                SendFormattedMessageToLogins(competition.Competitors.Select(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z is now the leader of the competition. The new competition name is {[#HighlightStyle]}{[Login]}{[#MessageStyle]}.", "Nickname", GetNickname(newLeader, true), "Login", newLeader);
                return;
            }

            competition.Leave(login);
            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} left the competition.", "Nickname", GetNickname(login, true));

            if (competition.Competitors.Count == 1)
            {
                SendFormattedMessageToLogin(competition.Leader, "{[#ServerStyle]}>{[#MessageStyle]} All competitors have left the competition. Competition was stopped.");
                RunningCompetitions.Remove(competition);
            }
        }

        private void HandleStopBattleCommand(string login)
        {
            Competition competition = RunningCompetitions.GetCompetitionByLogin(login);

            if (competition == null || !competition.IsLeader(login))
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} You're not the leader of the competition your taken part. So you cant stop the competition.");
                return;
            }

            SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} $z{[Nickname]}$z{[#MessageStyle]} has stopped the competition.", "Nickname", GetNickname(login, true));

            if (competition.Started && competition.Competitors.Count > 1)
                SendFormattedMessageToLogins(competition.Competitors.ConvertAll(c => c.Login).ToArray(), "{[#ServerStyle]}>{[#MessageStyle]} Competition is over here are the rankings:\n{[Rankings]}", "Rankings", GetRankingText(competition));

            RunningCompetitions.Remove(competition);
        }

        private void HandleShowBattlesCommand(string login)
        {
            if (RunningCompetitions.Count == 0)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Currently there are no competitions!");
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Current competitions:\n$z{[Competitions]}", "Competitions", GetCompetitionListString());
        }

        private string GetCompetitionListString()
        {
            StringBuilder result = new StringBuilder();

            foreach (Competition competition in RunningCompetitions)
            {
                result.AppendFormat("- {0} [State:{1}] [Players:{2}] [Rounds:{3}/{4}] [Password:{5}]\n", competition.Name, competition.Started ? "running" : "idle", competition.Competitors.Count, competition.DrivenRounds, competition.RoundLimit, competition.IsPasswordProtected ? "yes" : "no");
            }

            return result.ToString().TrimEnd();
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.CreateBattle, "Creates a competition. The RoundLimit-Parameter is optional as well as the JoinPassword for protecting the competition join.", "/CreateBattle [<RoundLimit>] [<JoinPassword>]", "'/CreateBattle 5' or '/CreateBattle TestPassword' or '/CreateBattle 5 TestPassword'"),
                    new CommandHelp(Command.JoinBattle, "Joins a battle with the specified name and the optional join password. When creating a battle the login of the creator is used as the name of the competition.", "/JoinBattle <CompetitionName> [<Password>]", "'/JoinBattle scordo' or '/JoinBattle scordo JoinPassword'"),
                    new CommandHelp(Command.LeaveBattle, "This command is used to leave the competition you're currently taking part in.", "/LeaveBattle", "/LeaveBattle"),
                    new CommandHelp(Command.StartBattle, "Starts the created competition. From now on score is counted for rankings. Joining the competition is no longer possible.", "/StartBattle", "/StartBattle"),
                    new CommandHelp(Command.StopBattle, "Stops the current competition.", "/StopBattle", "/StopBattle"),
                    new CommandHelp(Command.ShowBattles, "Shows a list of all created battles.", "/ShowBattles", "/ShowBattles"),
                };
            }
        }

        #endregion
    }
}
