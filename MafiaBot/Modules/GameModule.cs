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
		public async Task StartGameAsync(string nameset = "default")
		{
			try
			{
				if (!GameService.gamesInProgress.ContainsKey(Context.Guild.Id) || !GameService.gamesInProgress[Context.Guild.Id])
				{
					await Context.Channel.SendMessageAsync("There's a game of Mafia that will be starting in #mafia soon. You must be in that channel to play!");
					await service.StartGame(this.Context, nameset);
				}
			}
			catch (Exception e) { Console.WriteLine(e); }
		}

		[Command("vote")]
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
		public void Hanging()
		{
			if (Context.User.Id == GameService.mayors[Context.Guild.Id].Id)
				GameService.cancelTokens[Context.Guild.Id].Cancel();
		}
	}
}
