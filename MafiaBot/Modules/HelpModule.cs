using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace MafiaBot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		CommandService _commands;

		public HelpModule(CommandService commands)
		{
			_commands = commands;
		}

		[Command("help")]
		[Summary("Explains all the commands.")]
		public async Task Help(string command = "")
		{
			EmbedBuilder embed = new EmbedBuilder();

			foreach (ModuleInfo m in _commands.Modules)
			{
				foreach (CommandInfo c in m.Commands)
				{
					if(command == "" || c.Name == command)
						embed.AddField(c.Name, c.Summary);
				}
			}

			await ReplyAsync(embed: embed.Build());
		}
	}
}
