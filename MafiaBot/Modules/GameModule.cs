using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Commands;
using MafiaBot.Services;
using MafiaBot.Utils;

namespace MafiaBot.Modules
{
	public class GameModule : ModuleBase<SocketCommandContext>
	{
		private readonly GameService service;

		public GameModule(GameService _service)
		{
			service = _service;
		}

		[Command("startgame")]
		[Summary("Begins a game of Mafia")]
		public async Task StartGameAsync(string nameset = "default")
		{
			if (!GameService.gamesInProgress.ContainsKey(Context.Guild.Id) || !GameService.gamesInProgress[Context.Guild.Id])
			{
				await Context.Channel.SendMessageAsync("There's a game of Mafia that will be starting soon. You must be in that channel to play!");

				if ((nameset == "default" || nameset == "mafia")
				   || Settings.ReadSetting(DBUtils.CheckDBsForServer(Context.Guild.Id.ToString()), "namesets") == "true")
					await service.StartGame(this.Context, nameset);
				else
				{
					await ReplyAsync(Context.User.Mention + ", please note: This server prohibits the use of custom namesets, so the default will be used.");
					await service.StartGame(this.Context, "default");
				}
			}
		}

		[Command("vote")]
		[Summary("A command for use in game, which allows players to vote for things.")]
		public async Task Vote(IUser user)
		{
			if (user.Id != Context.User.Id)
			{
				Console.WriteLine(Context.Channel.Id);
				if (GameService.votingChannels.Contains(Context.Channel.Id))
				{
					GameService.votes[Context.Guild.Id].Add(user);
					await GuildUtils.SetAccessPermissions((RestTextChannel)Context.Channel, new IUser[] { user }, GuildUtils.CANT_SEND);
				}
			}
		}

		[Command("hanging")]
		[Summary("A command for use in game by the mayor, which causes a hanging.")]
		public async Task Hanging()
		{
			if (Context.User.Id == GameService.mayors[Context.Guild.Id].Id)
				GameService.cancelTokens[Context.Guild.Id].Cancel();
		}
	}
}
