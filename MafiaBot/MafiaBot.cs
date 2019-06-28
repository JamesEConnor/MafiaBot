using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MafiaBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MafiaBot
{
	public class MafiaBot
	{
		//Delimeter and client that can only be set by main program runner.
		public static string delimeter = "";
		public static bool DEBUG_MODE = false;
		public static DiscordSocketClient client { get; private set; }

		CommandService commands;

		public static void Main(string[] args)
		=> new MafiaBot().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			client = new DiscordSocketClient();
			client.Log += Log;

			await Login(client);

			commands = new CommandService(new CommandServiceConfig
			{
				LogLevel = LogSeverity.Verbose
			});

			IServiceProvider services = BuildServiceProvider();

			services.GetRequiredService<GameService>();
			await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

			await Task.Delay(-1);
		}

		public async Task Login(DiscordSocketClient client)
		{
			//Load all settings from the configuration file.
			//For more information on what the config file should look like,
			//check out the ReadMe.
			string[] str = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/config.txt");
			Dictionary<string, string> configs = new Dictionary<string, string>();
			foreach (string line in str)
			{
				//Split the key and value from the config file between the delimeter.
				string[] split = line.Split(new char[] { ':' }, 2);
				configs.Add(split[0], split[1]);
			}

			//Login and start.
			delimeter = configs["delimeter"];
			DEBUG_MODE = bool.Parse(configs["debug"]);
			await client.LoginAsync(TokenType.Bot, configs["token"]);
			await client.StartAsync();
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton(client)
			.AddSingleton(commands)
			.AddSingleton<GameService>()
			.AddSingleton<CommandHandler>()
			.BuildServiceProvider();
	}
}
