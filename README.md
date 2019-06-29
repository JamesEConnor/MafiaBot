# MafiaBot
A bot submitted for Discord's Hackweek. It runs a game of Mafia and can be customized with different "name sets" that are in JSON format. It's delimeter is "?" by default.

## Setting up Mafia Bot on your own Device
No matter how you choose to set up MafiaBot, you must make sure to set up the configuration file correctly. A 'config.txt' file must be created within the Base Directory of the program, and must contain the following values:

```
token:[INSERT DISCORD-PROVIDED DEVELOPER TOKEN]
delimeter:?
debug:false
```
The token is obtainable from the Discord Developer Portal. The delimeter can be set to whatever you like, and debug should be kept to false.

### Building From Source
This code was written and built using Xamarin Studio, a release of Monodevelop. While understanding that Monodevelop is outdated and a little obsolete, I prefer it's workflow over Visual Studio. As a result, I can only vouch for it's being built from Monodevelop, though I'm sure it works in Visual Studio perfectly fine. Simply download the files, open them up, and build. If you run into issues with packages, it currently uses Discord.NET and all of it's dependencies.

### Using the Pre-Built Version
To use the pre-built version (our most(ly) stable release), simply download the "Example Build" zip file. Unzip it, setup the configuration file as described above, and run MafiaBot.exe. We currently do not have an OSX build and, due to hardware limitations, unfortunately do not have one in the foreseeable future.

## Commands
(Parentheses) signify optional parameters while [brackets] signify required parameters.

* help          (command)  - Displays a description of all command functions or a specific one, if provided.
* startgame     [name set] - Creates a game of Mafia, as well as any necessary channels or roles.
* set           [setting]  - Modifies a setting for the server. **This command can only be run by the server owner.**
* listsettings             - Lists all possible settings.
* createnameset [name][mafia][townsperson][doctor][detective][mayor] - Creates a name set called 'name' with the provided descriptions.
* vote          [user]     - Registers your vote for a specific user during a game. You will be prompted to use this command during a game.
* hanging                  - This command can only be used by the mayor during a game, and only at certain times. When used, it causes a vote to take place on who to hang.

## Settings
* Max Players [maxplayers] - The maximum number of players allowed in a game of Mafia. Any value less than or equal to 0 is the same as none. The default is -1.
* Minimum Players [minplayers] - The minimum number of players required to start a game. This must be at least 5, and defaults to that value.
* Custom name sets allowed. [namesets] - Determines whether or not custom name sets are allowed to be used.
* Time to Start [timetostart] - How much time (in seconds) between when a user calls that "startgame" command and when the game actually starts. This defaults at 60 to ensure that players have enough time to get into the channel before the game begins.

## Creating Name Sets
> This section has been deprecated since the implementation of the 'createnameset' command. However, it remains for posterity.

To create a nameset, navigate to the 'name-sets' folder in the program's Base Directory. Then, create a JSON file with the name of the new nameset. Simply copy the template below into this file, replacing each of the '${}'s with their proper values.

```
{
	'mafia': '${mafia}',
	'townsperson': '${townsperson}',
	'cop': '${cop}',
	'doctor': '${doctor}',
	'mayor': '${mayor}'
}
```
## Known Issues
Right now, due to how Discord's permissions work, the Server Owner is unable to play as they can view every single channel. Because of time constraints, I was unable to come up with a way around this. See *Future Development*.

# Future Development
On the roadmap ahead, there are of course some ideas available. If you would like to contribute to MafiaBot, here are some things that you could try at:

- [ ] Create a system that allows the Server Owner to also play while not being able to view channels.
- [ ] Creating different variants of the game (i.e. One Night: Ultimate Werewolf, Epicmafia). This would require a large amount of re-programming, and might even have to become a different repository. But, imagine if it could all be kept in one bot?
- [ ] OSX Build. Due to hardware constraints, I am currently unable to create an OSX build in the foreseeable future, but it remains as a goal for MafiaBot.

If you're interested in contributing to MafiaBot, and need clarification of code, open up an issue. I'd be more than happy to help! As this is part of Discord Hackweek, I'll try to start commenting it and making it clearer after the end of the competition.
