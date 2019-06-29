using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using MafiaBot.Utils;
using Newtonsoft.Json;

namespace MafiaBot.Services
{
	public class GameService
	{
		public enum WinCondition
		{
			MafiaWin,
			TownspeopleWin,
			None
		}

		private readonly IServiceProvider _provider;
		private readonly DiscordSocketClient _client;
		private readonly CommandHandler _commands;

		public static Dictionary<ulong, bool> gamesInProgress = new Dictionary<ulong, bool>();

		public static Dictionary<ulong, List<IUser>> votes = new Dictionary<ulong, List<IUser>>();
		public static List<ulong> votingChannels = new List<ulong>();

		public static Dictionary<ulong, IUser> mayors = new Dictionary<ulong, IUser>();
		public static Dictionary<ulong, CancellationTokenSource> cancelTokens = new Dictionary<ulong, CancellationTokenSource>();


		public GameService(IServiceProvider provider, DiscordSocketClient client, CommandHandler commands)
		{
			_provider = provider;
			_client = client;
			_commands = commands;
		}

		public async Task StartGame(SocketCommandContext context, string nameset)
		{
			if (gamesInProgress.ContainsKey(context.Guild.Id))
				gamesInProgress[context.Guild.Id] = true;
			else
				gamesInProgress.Add(context.Guild.Id, true);

			string settings = DBUtils.CheckDBsForServer(context.Guild.Id.ToString());

			if (GuildUtils.NumberOfOnlineUsers(context.Guild) < int.Parse(Settings.ReadSetting(settings, "minplayers")) && !MafiaBot.DEBUG_MODE)
			{
				await context.Channel.SendMessageAsync("\u274C Looks like the Mafia's finished everyone off. Try again when more people are online!");
				return;
			}

			FileStream fs = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/name-sets/" + nameset + ".json");
			StreamReader reader = new StreamReader(fs);
			Nameset names = JsonConvert.DeserializeObject<Nameset>(reader.ReadToEnd());

			fs.Close();
			fs.Dispose();

			reader.Close();
			reader.Dispose();


			//CREATE CATEGORY
			SocketCategoryChannel cat = await GuildUtils.AddCategoryChannel("Mafia Game Channels", context.Guild);
			await GuildUtils.RemoveAllCategoryChannels(cat);
			List<RestTextChannel> channels = new List<RestTextChannel>();

			//SETTING UP CHANNELS ---------------------
			RestTextChannel town = await GuildUtils.AddChannel("Town", context.Guild, catergory: cat.Id);
			await GuildUtils.SetAccessPermissions(town, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.CANT_SEND);
			channels.Add(town);

			RestTextChannel mafia = await GuildUtils.AddChannel(names.mafia, context.Guild, catergory: cat.Id);
			await GuildUtils.SetAccessPermissions(mafia, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.CANT_SEND);
			await GuildUtils.SetAccessPermissions(mafia, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.BANNED_PERMISSIONS);
			channels.Add(mafia);

			RestTextChannel doctor = await GuildUtils.AddChannel(names.doctor, context.Guild, catergory: cat.Id);
			await GuildUtils.SetAccessPermissions(doctor, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.CANT_SEND);
			await GuildUtils.SetAccessPermissions(doctor, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.BANNED_PERMISSIONS);
			channels.Add(doctor);

			RestTextChannel detective = await GuildUtils.AddChannel(names.cop, context.Guild, catergory: cat.Id);
			await GuildUtils.SetAccessPermissions(detective, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.CANT_SEND);
			await GuildUtils.SetAccessPermissions(detective, (new List<IUser>(context.Guild.Users)).ToArray(), GuildUtils.BANNED_PERMISSIONS);
			channels.Add(detective);



			CreateGame(context, settings, channels.ToArray(), names);

			return;
		}

		private async Task CreateGame(SocketCommandContext context, string settings, RestTextChannel[] channels, Nameset names)
		{
			int milliseconds = int.Parse(Settings.ReadSetting(settings, "timetostart")) * 1000;
			await channels[0].SendMessageAsync("@here There's a game of mafia starting in " + (milliseconds / 1000) + " seconds!");
			await Task.Delay(milliseconds / 2);
			await channels[0].SendMessageAsync((milliseconds / 2000) + " seconds until the game starts!");
			await Task.Delay(milliseconds / 2);

			if (int.Parse(Settings.ReadSetting(settings, "maxplayers")) > await GuildUtils.NumberOfOnlineUsers(channels[0]))
			{
				await channels[0].SendMessageAsync("According to server settings, there can only be " + Settings.ReadSetting(settings, "maxplayers") + " players in the game. Anyone in this channel is considered a player, so some will have to go. Think something's wrong? Talk to the server owner...");
				return;
			}

			//0 = Townspeople, 1 = Mafia, 2 = Detective, 3 = Doctor
			IUser[][] lists = GenerateUserLists(context.Guild.GetChannel(channels[0].Id));


			await GuildUtils.SetAccessPermissions(channels[1], lists[1]);
			await GuildUtils.SetAccessPermissions(channels[3], lists[2]);
			await GuildUtils.SetAccessPermissions(channels[2], lists[3]);

			if (lists[2][0] != null)
				await channels[3].SendMessageAsync(Messages.DETECTIVE_STARTING_MESSAGE.UseNameset(names));
			if (lists[3][0] != null)
				await channels[2].SendMessageAsync(Messages.DOCTOR_STARTING_MESSAGE.UseNameset(names));

			await channels[0].SendMessageAsync(Messages.TOWNSPEOPLE_STARTING_MESSAGE.UseNameset(names));
			await channels[1].SendMessageAsync(Messages.MAFIA_STARTING_MESSAGE.UseNameset(names));

			if (!cancelTokens.ContainsKey(context.Guild.Id))
				cancelTokens.Add(context.Guild.Id, new CancellationTokenSource());

			RunGame(context, settings, channels, names, lists);
		}

		//Channels
		//0 = town, 1 = mafia, 2 = doctor, 3 = detective
		//User Lists
		//0 = Townspeople, 1 = Mafia, 2 = Detective, 3 = Doctor
		private async Task RunGame(SocketCommandContext context, string settings, RestTextChannel[] channels, Nameset names, IUser[][] users)
		{
			try
			{
				IUser[] allUsers = users[0].Concat(users[1]).Concat(users[2]).Concat(users[3]).ToArray();
				bool gameOver = false;

				await Task.Delay(25000);

				while (!gameOver)
				{
					//0 = killed, 1 = saved
					IUser[] results = await Night(context, settings, channels, names, users, allUsers);
					if (results[0].Id != results[1].Id)
					{
						if (MafiaBot.DEBUG_MODE)
							Console.WriteLine(results[0] + " - " + results[1] + " - " + allUsers.Length);

						allUsers = allUsers.Remove(results[0]);
						RemoveUser(ref users, results[0], names);
					}

					WinCondition condition = CheckWinCondition(users, ref gameOver);
					if (condition == WinCondition.MafiaWin)
					{
						await channels[0].SendMessageAsync(Messages.MAFIA_WIN_MESSAGE.UseNameset(names));
						break;
					}
					else if (condition == WinCondition.TownspeopleWin)
					{
						await channels[0].SendMessageAsync(Messages.TOWNSPEOPLE_WIN_MESSAGE.UseNameset(names));
						break;
					}

					await Task.Delay(5000);

					if (!mayors.ContainsKey(context.Guild.Id) || results[0].Id == mayors[context.Guild.Id].Id)
					{
						IUser mayor = await RunVote(context.Guild, channels[0], 30, Messages.MAYORAL_ELECTION_MESSAGE.UseNameset(names), allUsers, allUsers);
						await channels[0].SendMessageAsync("The votes are in and the electoral college has decided. Congratulations " + mayor.Mention + "! You're the new mayor!!");
						if (mayors.ContainsKey(context.Guild.Id))
							mayors[context.Guild.Id] = mayor;
						else
							mayors.Add(context.Guild.Id, mayor);
					}

					await channels[0].SendMessageAsync(mayors[context.Guild.Id].Mention + ": will there be a hanging today? If so, type '?hanging' within 20 seconds to start a vote.");

					await Task.Delay(20000, cancelTokens[context.Guild.Id].Token);

					if (cancelTokens[context.Guild.Id].IsCancellationRequested)
					{
						IUser hanged = await RunVote(context.Guild, channels[0], 30, Messages.HANGING_VOTE.UseNameset(names), allUsers, allUsers);
						allUsers = allUsers.Remove(hanged);
						await channels[0].SendMessageAsync(RemoveUser(ref users, hanged, names));

						condition = CheckWinCondition(users, ref gameOver);

						if (condition == WinCondition.MafiaWin)
							await channels[0].SendMessageAsync(Messages.MAFIA_WIN_MESSAGE.UseNameset(names));
						else if (condition == WinCondition.TownspeopleWin)
							await channels[0].SendMessageAsync(Messages.TOWNSPEOPLE_WIN_MESSAGE.UseNameset(names));
					}
					else
					{
						await GuildUtils.SendEmbed(channels[0], context, "Fine coward, I guess there's no trial... Now I'm disappointed. Whelp, time for another round of the old-fashioned killing...", Color.Green);
					}
				}

				mayors.Remove(context.Guild.Id);
				votes.Remove(context.Guild.Id);

				gamesInProgress.Remove(context.Guild.Id);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public async Task<IUser[]> Night(SocketCommandContext context, string settings, RestTextChannel[] channels, Nameset names, IUser[][] users, IUser[] allUsers)
		{
			RestTextChannel townChannel = channels[0];
			RestTextChannel mafiaChannel = channels[1];
			RestTextChannel doctorChannel = channels[2];
			RestTextChannel detectiveChannel = channels[3];

			IUser[] townspeopleUsers = users[0];
			IUser[] mafiaUsers = users[1];
			IUser[] detectiveUser = users[2];
			IUser[] doctorUser = users[3];



			await GuildUtils.SendEmbed(townChannel, context, "Hang tight! The " + names.mafia + " are choosing who to kill!", Color.Blue);

			try
			{
				IUser killed = await RunVote(context.Guild, mafiaChannel, 60, Messages.MAFIA_VOTE_MESSAGE.Replace("${delimeter}", MafiaBot.delimeter).UseNameset(names), users[1], allUsers);
				await channels[1].SendMessageAsync(killed.Username + " will be killed...");

				await GuildUtils.SendEmbed(townChannel, context, "Hang tight! The " + names.doctor + " is choosing who to save!", Color.Blue);
				IUser saved = await RunVote(context.Guild, doctorChannel, 40, Messages.DOCTOR_VOTE_MESSAGE.Replace("${delimeter}", MafiaBot.delimeter).UseNameset(names), doctorUser, allUsers);
				await channels[3].SendMessageAsync(saved.Username + " will be saved...");

				await GuildUtils.SendEmbed(townChannel, context, "Hang tight! The " + names.cop + " is learning someone's role!", Color.Blue);
				IUser learned = await RunVote(context.Guild, detectiveChannel, 40, Messages.DETECTIVE_VOTE_MESSAGE.Replace("${delimeter}", MafiaBot.delimeter).UseNameset(names), detectiveUser);
				Console.WriteLine(learned);
				if (learned != null)
				{
					if (mafiaUsers.Contains(learned))
						await detectiveChannel.SendMessageAsync(learned.Username + " is part of the " + names.mafia);
					else if (doctorUser.Contains(learned))
						await detectiveChannel.SendMessageAsync(learned.Username + " is the " + names.doctor);
					else
						await detectiveChannel.SendMessageAsync(learned.Username + " is a " + names.townsperson);
				}

				if (killed.Id != saved.Id)
				{
					string roleReveal = "";

					if (mafiaUsers.Contains(killed))
					{
						roleReveal = "They were a " + names.mafia + ".";
						mafiaUsers = mafiaUsers.Remove(killed);
					}
					else if (detectiveUser.Contains(killed))
					{
						roleReveal = "They were a " + names.cop + ".";
						detectiveUser = detectiveUser.Remove(killed);
					}
					else if (doctorUser.Contains(killed))
					{
						roleReveal = "They were a " + names.doctor + ".";
						doctorUser = doctorUser.Remove(killed);
					}
					else
					{
						roleReveal = "They were a " + names.townsperson + ".";
						townspeopleUsers = townspeopleUsers.Remove(killed);
					}

					await GuildUtils.SendEmbed(townChannel, context, Messages.GenerateDeathMessage(killed, roleReveal));
				}
				else
				{
					await GuildUtils.SendEmbed(townChannel, context, Messages.GenerateSaveMessage(killed), new Color(255, 255, 255));
				}

				return new IUser[] { killed, saved };
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		
		private async Task<IUser> RunVote(SocketGuild guild, RestTextChannel channel, int seconds, string message, IUser[] users, IUser[] randomSelection = null)
		{
			if (votes.ContainsKey(guild.Id))
				votes[guild.Id] = new List<IUser>();
			else
				votes.Add(guild.Id, new List<IUser>());

			if (!votingChannels.Contains(channel.Id))
				votingChannels.Add(channel.Id);
			
			await GuildUtils.SetAccessPermissions(channel, users, GuildUtils.CAN_SEND);

			await channel.SendMessageAsync(message);
			RestUserMessage timeMessage = await channel.SendMessageAsync(seconds + " seconds left.");

			for (int a = 0; a < seconds; a++)
			{
				await timeMessage.ModifyAsync(msg => msg.Content = (seconds - a) + " seconds left.");
				await Task.Delay(1000);
			}

			await GuildUtils.SetAccessPermissions(channel, users, GuildUtils.CANT_SEND);
			votingChannels.Remove(channel.Id);

			if(MafiaBot.DEBUG_MODE)
				Console.WriteLine(votes[guild.Id].Count + ":" + (randomSelection == null) + ":" + (randomSelection == null ? -1 : randomSelection.Length));

			if (votes[guild.Id].Count > 0)
				return votes[guild.Id].MostCommon();
			else if(randomSelection != null)
				return randomSelection[(new Random()).Next(0, randomSelection.Length)];

			return null;
		}

		private static WinCondition CheckWinCondition(IUser[][] users, ref bool gameOver)
		{
			gameOver = true;

			if (users[0].Length <= 0 && users[2].Length <= 0 && users[3].Length <= 0)
				return WinCondition.MafiaWin;
			else if (users[1].Length <= 0)
				return WinCondition.TownspeopleWin;

			gameOver = false;

			return WinCondition.None;
		}

		private static string RemoveUser(ref IUser[][] users, IUser rem, Nameset names)
		{
			if (users[1].Contains(rem))
			{
				users[1] = users[1].Remove(rem);
				return rem.Mention + " was hanged! They were a " + names.mafia;
			}
			else if (users[2].Contains(rem))
			{
				users[2] = users[2].Remove(rem);
				return rem.Mention + " was hanged! They were a " + names.cop;
			}
			else if (users[3].Contains(rem))
			{
				users[3] = users[3].Remove(rem);
				return rem.Mention + " was hanged! They were a " + names.doctor;
			}
			else
			{
				users[0] = users[0].Remove(rem);
				return rem.Mention + " was hanged! They were a " + names.townsperson;
			}
		}

		private IUser[][] GenerateUserLists(SocketChannel channel)
		{
			try
			{
				IUser[][] result = new IUser[4][];

				List<SocketUser> users = new List<SocketUser>(channel.Users.Where((arg) => arg.Status == UserStatus.Online));

				int mafiaCount = users.Count / 3;
				result[1] = new IUser[mafiaCount];
				result[2] = new IUser[1];
				result[3] = new IUser[1];

				if (MafiaBot.DEBUG_MODE)
					Console.WriteLine("COUNT: " + users.Count + "---" + mafiaCount);

				Random r = new Random();
				int i = 0;
				for (int a = 0; a < mafiaCount; a++)
				{
					i = r.Next(0, users.Count);
					do
					{
						Console.WriteLine("1");
						result[1][a] = users[i];
						users.RemoveAt(i);
						i = r.Next(0, users.Count);
					} while (users.Count > 0 && users[i].IsBot && !MafiaBot.DEBUG_MODE);
				}

				i = r.Next(0, users.Count);
				do
				{
					Console.WriteLine("2");
					result[2][0] = users[i];
					users.RemoveAt(i);
					i = r.Next(0, users.Count);
				} while (users.Count > 0 && users[i].IsBot && !MafiaBot.DEBUG_MODE);

				i = r.Next(0, users.Count);
				do
				{
					Console.WriteLine("3");
					result[3][0] = users[i];
					users.RemoveAt(i);
					i = r.Next(0, users.Count);
				} while (users.Count > 0 && users[i].IsBot && !MafiaBot.DEBUG_MODE);

				result[0] = users.Where(x => (!x.IsBot && x.Status == UserStatus.Online) || MafiaBot.DEBUG_MODE).ToArray();

				return result;
			}
			catch (Exception e) { Console.WriteLine(e); return null; }
		}
	}
}
