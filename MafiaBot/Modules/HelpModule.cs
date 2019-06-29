using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MafiaBot.Utils;

namespace MafiaBot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		CommandService _commands;

		public HelpModule(CommandService commands)
		{
			_commands = commands;
		}


		[Command("createnameset")]
		[Summary("Creates a new name set for use in games. This will migrate across all servers using the bot.")]
		public async Task CreateNameSet(string name, string mafia, string townsperson, string doctor, string detective, string mayor)
		{
			Nameset nameset = new Nameset
			{
				mafia = mafia,
				townsperson = townsperson,
				doctor = doctor,
				cop = detective,
				mayor = mayor
			};

			if (!Messages.SaveNameset(nameset, name))
				await ReplyAsync("A nameset with the name '" + name + "' already exists. Pick a new name and try again...");
			else
				await ReplyAsync("A nameset with the name " + name + " has now been created! Just type '" + MafiaBot.delimeter + "startgame " + name + "' to use it!");
		}


		[Command("help")]
		[Summary("Explains all the commands or a specific command, if 'command' argument is present.")]
		public async Task Help(string command = "")
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithDescription("For parameters, [] = required, () = optional");

			foreach (ModuleInfo m in _commands.Modules)
			{
				foreach (CommandInfo c in m.Commands)
				{
					if (command == "" || c.Name == command)
					{
						string name = c.Name + " ";
						foreach (ParameterInfo p in c.Parameters)
						{
							if (p.IsOptional)
								name += "(" + p.Name + ")";
							else
								name += "[" + p.Name + "]";
						}

						embed.AddField(name, c.Summary);
					}
				}
			}

			await ReplyAsync(embed: embed.Build());
		}
	}
}
