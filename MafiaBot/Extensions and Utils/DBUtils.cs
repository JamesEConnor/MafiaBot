using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace MafiaBot.Utils
{
	public class DBUtils
	{
		public const string DEFAULT_SETTINGS = "{maxplayers,-1},{minplayers,5},{namesets,true},{timetostart,60}";

		public static string CheckDBsForServer(string serverGUID)
		{
			FileStream fs;
			if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.csv"))
				fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");
			else
				fs = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");

			StreamReader reader = new StreamReader(fs);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] split = line.Split(':');
				if (split[0] == serverGUID)
				{
					reader.Close();
					reader.Dispose();

					fs.Close();
					fs.Dispose();

					return split[1];
				}
			}

			reader.Close();
			reader.Dispose();

			fs.Close();
			fs.Dispose();

			fs = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");

			StreamWriter writer = new StreamWriter(fs);
			writer.WriteLine(serverGUID + ":" + DEFAULT_SETTINGS);

			writer.Close();
			writer.Dispose();

			fs.Close();
			fs.Dispose();

			return DEFAULT_SETTINGS;
		}

		public static string WriteToServerDB(string serverGUID, string settings = DEFAULT_SETTINGS)
		{
			FileStream fs;
			if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.csv"))
			{
				fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");
				fs.Close();
				fs.Dispose();
			}




			string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");

			for (int a = 0; a < lines.Length; a++)
			{
				string[] split = lines[a].Split(':');
				if (split[0] == serverGUID)
				{
					lines[a] = serverGUID + ":" + settings;
					File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "/db.csv", lines);
					return settings;
				}
			}



			fs = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + "/db.csv");

			StreamWriter writer = new StreamWriter(fs);
			writer.WriteLine(serverGUID + ":" + DEFAULT_SETTINGS);

			writer.Close();
			writer.Dispose();

			fs.Close();
			fs.Dispose();

			return settings;
		}
	}

	public class GuildUtils
	{
		public static OverwritePermissions ACCESS_PERMISSIONS = new OverwritePermissions(1024, 456768);
		public static OverwritePermissions BANNED_PERMISSIONS = new OverwritePermissions(0, 66583616);
		public static OverwritePermissions CANT_SEND = new OverwritePermissions(0, 6144);
		public static OverwritePermissions CAN_SEND = new OverwritePermissions(6144, 0);

		public static async Task<RestTextChannel> AddChannel(string name, SocketGuild guild, string topic = "", ulong catergory = 0, bool isNSFW = false)
		{
			return await guild.CreateTextChannelAsync(name, x =>
			{
				x.Topic = topic;
				x.CategoryId = catergory;
				x.IsNsfw = isNSFW;
			});
		}

		public static async Task<SocketCategoryChannel> AddCategoryChannel(string name, SocketGuild guild)
		{
			foreach (SocketCategoryChannel cat in guild.CategoryChannels)
				if (cat.Name == name)
					return cat;

			await guild.CreateCategoryChannelAsync(name);
			foreach (SocketCategoryChannel cat in guild.CategoryChannels)
				if (cat.Name == name)
					return cat;

			return null;
		}

		public static async Task RemoveAllCategoryChannels(SocketCategoryChannel cat)
		{
			foreach (SocketGuildChannel chan in cat.Channels)
				await chan.DeleteAsync();
		}

		public static async Task SetAccessPermissions(RestTextChannel channel, IUser[] users)
		{
			foreach (IUser user in users)
				await channel.AddPermissionOverwriteAsync(user, ACCESS_PERMISSIONS);
		}

		public static async Task SetAccessPermissions(RestTextChannel channel, IUser[] users, OverwritePermissions perms)
		{
			foreach (IUser user in users)
			{
				if(user != null)
					await channel.AddPermissionOverwriteAsync(user, perms);
			}
		}

		public static async Task RemoveAllPermissions(RestTextChannel channel)
		{
			IEnumerable<RestGuildUser> users = await channel.GetUsersAsync().FlattenAsync();
			foreach (RestGuildUser user in users)
				await channel.RemovePermissionOverwriteAsync(user);
		}

		public static int NumberOfOnlineUsers(SocketGuild guild)
		{
			int count = 0;
			foreach (IUser user in guild.Users)
				if (user.Status == UserStatus.Online)
					count++;

			return count;
		}

		public static async Task<int> NumberOfOnlineUsers(RestTextChannel channel)
		{
			int count = 0;
			IEnumerable<RestGuildUser> users = await channel.GetUsersAsync().FlattenAsync();
			foreach (RestGuildUser user in users)
				if (user.Status == UserStatus.Online)
					count++;

			return count;
		}

		public static async Task SendEmbed(ITextChannel channel, SocketCommandContext context, string message)
		{
			EmbedBuilder builder = new EmbedBuilder();
			Embed embed = builder.WithDescription(message).WithColor(Color.DarkRed).Build();
			await channel.SendMessageAsync(embed: embed);
		}

		public static async Task SendEmbed(ITextChannel channel, SocketCommandContext context, string message, Color c)
		{
			EmbedBuilder builder = new EmbedBuilder();
			Embed embed = builder.WithDescription(message).WithColor(c).Build();
			await channel.SendMessageAsync(embed: embed);
		}
	}

	public class Settings
	{
		public static string ReadSetting(string settings, string settingName)
		{
			settings = settings.Remove(0, 1);
			settings = settings.Remove(settings.Length - 1, 1);
			string[] split = settings.Split(new string[] { "},{" }, StringSplitOptions.None);

			foreach (string el in split)
			{
				string[] keyValue = el.Split(',');
				if (keyValue[0] == settingName)
					return keyValue[1];
			}

			return null;
		}

		public static string WriteSetting(string settings, string settingName, string settingValue)
		{
			settings = settings.Remove(0, 1);
			settings = settings.Remove(settings.Length - 1, 1);
			string[] split = settings.Split(new string[] { "},{" }, StringSplitOptions.None);

			string result = "{";

			foreach (string el in split)
			{
				string[] keyValue = el.Split(',');
				if (keyValue[0] == settingName)
					keyValue[1] = settingValue;

				result += keyValue[0] + "," + keyValue[1] + "},{";
			}

			return result.Substring(0, result.Length - 2);
		}
	}

	public static class Messages
	{
		//Sent in town channel at beginning of game.
		public const string TOWNSPEOPLE_STARTING_MESSAGE =
			"Hello townspeople! Welcome to a game of Mafia. I'm your host MafiaBot! Hope you're all ready for some fun! Here are the rules: \n\n" +
			"\t\uD83D\uDD2B If you can see the ${mafia} channel, you're a ${mafia}. Your goal is to kill all of the townspeople.\n\n" +
			"\t\uD83C\uDFE0 If you can't see the ${mafia} channel, you're a ${townsperson}. Your goal is to figure out who the mafia are and hang them. More on that in a second.\n\n" +
			"\t\uD83D\uDE91 If you can see the ${doctor} channel, you're the ${doctor}. You can save one person from death each night. Make it count!\n\n" +
			"\t\uD83D\uDE94 If you can see the ${cop} channel, you're the ${cop}. You can learn one person's role each night.\n\n" +
			"So what do I mean by night? Well, the game cycles between two periods. Day, when townspeople can elect a mayor or choose to hang someone, and night, when all of the stuff mentioned above happens. Night lasts for two minutes, or until everyone with a special role has chosen a person, whichever comes first. If you don't have a special role, that's pretty much it. If you do, check out your respective channel or DM for more info. Let's get started!\n\n" +
			"P.S. Any votes that result in a tie will be determined randomly by me. Sorry, I don't make the rules.";

		//Sent in mafia channel at beginning of game.
		public const string MAFIA_STARTING_MESSAGE =
			"Welcome to the ${mafia} everyone! Your goal is to kill off every innocent person in the town (including the ${cop} and ${doctor}). Here's how it works:\n\n" +
			"\t\uD83C\uDF19 Each night, you can come to this channel and mention one (1) ${townsperson}. It won't work if they're part of the ${mafia}.\n\n" +
			"\t\uD83C\uDFD9 Once you've done that, head back to the town channel to watch the carnage unfold. The top vote or votes (depending on the server settings) will be killed.";

		//DM'd to Detective at beginning of game.
		public const string DETECTIVE_STARTING_MESSAGE =
			"Welcome to the police force ${cop}! Glad to have you. Here's how this works:\n\n" +
			"\t\uD83C\uDF19 Each night, you should DM me with the mention of one (1) ${townsperson}. I'll tell you what their role is (Either ${townsperson}, ${doctor}, or ${mafia}).\n\n" +
			"\t\uD83C\uDFD9 You can then go back to the town channel and do what you will with that information.";

		//DM'd to Doctor at beginning of game.
		public const string DOCTOR_STARTING_MESSAGE =
			"Ehhhhh, what's up doc? Here's how this works:\n\n" +
			"\t\uD83C\uDF19 Each night, you should DM me with the mention of one (1) ${townsperson} that you'd like to save.\n\n" +
			"\t\uD83C\uDFD9 You can then go back to the town channel and see what happens!";

		public const string MAFIA_VOTE_MESSAGE =
			"Time to choose someone to kill. Cast your (1) vote for someone not in the Mafia by typing '?vote @username' If this person is in the Mafia, it won't work!";

		public const string DOCTOR_VOTE_MESSAGE =
			"Time to choose someone to save. Cast your (1) vote for someone other than you by typing '?vote @username' If you vote for yourself, it won't work!";

		public const string DETECTIVE_VOTE_MESSAGE =
			"Time to learn someone's role. Cast your (1) vote for someone other than you by typing '?vote @username' If you vote for yourself, it won't work!";

		//Sent in town channel when mayoral election required.
		public const string MAYORAL_ELECTION_MESSAGE =
			"@here Alrighty everyone, gather up your megaphones and lawn signs, it's election time! Whoever holds this position gets a lot of power:\n\n" +
			"\t\u2694 After each night, the ${mayor} can decide if there will be a trial. If so, there will be a vote to see who will be hanged.\n\n" +
			"\t\u2611 So let's vote! Mention whoever you'd like to elect in the next 15 seconds!";

		public const string HANGING_VOTE =
			"Alrighty, you all decided that the best way to fight murder is murder. Well let's get this over with. Type '?vote @username' to vote for who you would like hanged, eliminated, murdered, etc. Vote for yourself, and it won't work. If no one votes, I get to decide... and no one wants that ;)";

		//Sent in town channel when the mafia win.
		public const string MAFIA_WIN_MESSAGE =
			"\uD83D\uDD75 @here Congratulations to the ${mafia}s! You've won!!!";

		//Sent in town channel when the townspeople win.
		public const string TOWNSPEOPLE_WIN_MESSAGE =
			"\uD83C\uDFE0 @here Congratulations to the ${townsperson}s! You've won!!!";

		public static string GenerateDeathMessage(IUser user, string roleReveal)
		{
			string[] messages = File.ReadAllLines(AppContext.BaseDirectory + "/messages/death-messages.txt");
			Random r = new Random();
			return messages[r.Next(0, messages.Length)].Replace("${username}", user.Mention) + "\n" +
				                                       user.Username + " was eliminated.\n" +
				                                       roleReveal;
		}

		public static string GenerateSaveMessage(IUser user)
		{
			string[] messages = File.ReadAllLines(AppContext.BaseDirectory + "/messages/save-messages.txt");
			Random r = new Random();
			return messages[r.Next(0, messages.Length)].Replace("${username}", user.Mention) + "\n" +
				                                       user.Username + " survived being a murder attempt.";
		}

		public static string UseNameset(this string str, Nameset ns)
		{
			return str.Replace("${townsperson}", ns.townsperson)
					  .Replace("${mafia}", ns.mafia)
					  .Replace("${cop}", ns.cop)
					  .Replace("${doctor}", ns.doctor)
					  .Replace("${mayor}", ns.mayor);
		}

		public static bool SaveNameset(this Nameset names)
		{
			if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/name-sets/" + names.mafia + ".json"))
				return false;

			FileStream fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + "/name-sets/" + names.mafia + ".json");
			StreamWriter writer = new StreamWriter(fs);
			writer
		}
	}

	public static class Extensions
	{
		public static T MostCommon<T>(this IEnumerable<T> list)
		{
			return list.GroupBy(i => i).OrderByDescending(grp => grp.Count())
				       .Select(grp => grp.Key).First();
		}

		public static T[] Remove<T>(this T[] array, T item)
		{
			List<T> result = new List<T>();
			result.Remove(item);
			return result.ToArray();
		}
	}
}

public class Nameset
{
	public string mafia;
	public string townsperson;
	public string cop;
	public string doctor;
	public string mayor;
}