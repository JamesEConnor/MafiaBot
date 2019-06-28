using System;
using System.Threading.Tasks;
using Discord.Commands;
using MafiaBot.Utils;

namespace MafiaBot.Modules
{
	public class SettingsModule : ModuleBase<SocketCommandContext>
	{
		[Command("set")]
		[Summary("Changes a setting for the server. Can only be executed by the server owner.")]
		public async Task SetSetting(string settingName, string settingValue)
		{
			if (Context.User.Id == Context.Guild.OwnerId)
			{
				string settings = DBUtils.CheckDBsForServer(Context.Guild.Id.ToString());
				settings = Settings.WriteSetting(settings, settingName, settingValue);
				DBUtils.WriteToServerDB(Context.Guild.Id.ToString(), settings);
			}
			else
			{
				await ReplyAsync("The 'set' command can only be used by the server's owner!");
			}
		}
	}
}
