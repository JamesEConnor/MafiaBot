using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MafiaBot.Utils;

namespace MafiaBot.Modules
{
	public class SettingsModule : ModuleBase<SocketCommandContext>
	{
		[Command("listsettings")]
		[Summary("A list of all possible settings.")]
		public async Task ListSettings()
		{
			string settings = DBUtils.SETTING_DESCRIPTIONS;
			settings = settings.Substring(1, settings.Length - 2);
			string[] split = settings.Split(new string[] { "},{" }, StringSplitOptions.None);

			EmbedBuilder builder = new EmbedBuilder();
			foreach (string str in split)
				builder.AddField(str.Split(',')[0], str.Split(',')[1]);

			await ReplyAsync(embed: builder.Build());
		}

		[Command("set")]
		[Summary("Changes a setting for the server. Can only be executed by the server owner. To get a full list of possible settings, use listsettings.")]
		public async Task SetSetting(string settingName, string settingValue)
		{
			if (Context.User.Id == Context.Guild.OwnerId)
			{
				string requirement = Settings.ReadSetting(DBUtils.SETTING_TYPE_REQUIREMENTS, settingName);
				if (requirement != null)
				{
					bool b = false;
					int i = 0;
					if (requirement == "BOOL" && !bool.TryParse(settingValue.ToLower(), out b))
					{
						await ReplyAsync(settingName + " must be either 'true' or 'false'");
						return;
					}
					else if (requirement.Contains("INT"))
					{
						if (!int.TryParse(settingValue, out i))
						{
							await ReplyAsync(settingName + " must be an integer.");
							return;
						}

						if (requirement.Contains("G"))
						{
							int g = int.Parse(requirement.Replace("INTG", ""));
							if (i < g)
							{
								await ReplyAsync(settingName + " must be an integer greater than or equal to " + g);
								return;
							}
						}
					}
				}

				string settings = DBUtils.CheckDBsForServer(Context.Guild.Id.ToString());
				settings = Settings.WriteSetting(settings, settingName.ToLower(), settingValue.ToLower());
				DBUtils.WriteToServerDB(Context.Guild.Id.ToString(), settings);

				await ReplyAsync(settingName + " changed to " + settingValue);
			}
			else
			{
				await ReplyAsync("The 'set' command can only be used by the server's owner!");
			}
		}
	}
}
