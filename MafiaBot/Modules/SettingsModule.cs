using System;
using Discord.Commands;
using MafiaBot.Utils;

namespace MafiaBot.Modules
{
	public class SettingsModule : ModuleBase<SocketCommandContext>
	{
		[Command("set")]
		public void SetSetting(string settingName, string settingValue)
		{
			try
			{
				string settings = DBUtils.CheckDBsForServer(Context.Guild.Id.ToString());
				settings = Settings.WriteSetting(settings, settingName, settingValue);
				DBUtils.WriteToServerDB(Context.Guild.Id.ToString(), settings);
			}
			catch (Exception e) { Console.WriteLine(e); }
		}
	}
}
