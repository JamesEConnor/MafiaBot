using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MafiaBot;
using MafiaBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MafiaBot
{
	public class CommandHandler
	{
		public readonly CommandService commands;
		public readonly IServiceProvider services;

		public CommandHandler(IServiceProvider _services, CommandService _commands)
		{
			commands = _commands;
			services = _services;
		}

		public async Task InstallCommandsAsync()
		{
			IEnumerable<ModuleInfo> result = await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
			foreach (var r in result)
				foreach (var c in r.Commands)
					Console.WriteLine(c.Name);
			MafiaBot.client.MessageReceived += HandleCommandsAsync;
		}

		private async Task HandleCommandsAsync(SocketMessage messageParameter)
		{
			SocketUserMessage message = messageParameter as SocketUserMessage;
			if (message == null)
				return;

			int commandEndPos = 0;
			if (!(message.HasStringPrefix(MafiaBot.delimeter, ref commandEndPos) ||
				 message.HasMentionPrefix(MafiaBot.client.CurrentUser, ref commandEndPos)) ||
				 message.Author.IsBot)
				return;

			SocketCommandContext context = new SocketCommandContext(MafiaBot.client, message);
			IResult result = await commands.ExecuteAsync(context, commandEndPos, services);
			Console.WriteLine(result.ErrorReason);
		}
	}
}
