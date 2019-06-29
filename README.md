# MafiaBot
A bot submitted for Discord's Hackweek. It runs a game of Mafia and can be customized with different "name sets" that are in JSON format. It's delimeter is "?" by default.

### Setting Up Mafia Bot on your own Device

### Commands
(Parentheses) signify optional parameters while [brackets] signify required parameters.
```
help          (command)  - Displays a description of all command functions or a specific one, if provided.
startgame     [name set] - Creates a game of Mafia, as well as any necessary channels or roles.
set           [setting]  - Modifies a **setting** for the server. ```**```This command can only be run by the server owner.```**```
listsettings             - Lists all possible settings.
createnameset [name][mafia][townsperson][doctor][detective][mayor] - Creates a name set called 'name' with the
								     provided descriptions.
vote          [user]     - Registers your vote for a specific user during a game. You will be prompted to use
			   this command during a game.
hanging                  - This command can only be used by the mayor during a game, and only at certain times. 
			   When used, it causes a vote to take place on who to hang.
```

### Settings
* Max Players [maxplayers] - The maximum number of players allowed in a game of Mafia. Any value less than or equal to 0 is the same as none. The default is -1.
* Minimum Players [minplayers] - The minimum number of players required to start a game. This must be at least 5, and defaults to that value.
* Custom name sets allowed. [namesets] - Determines whether or not custom name sets are allowed to be used.
* Time to Start [timetostart] - How much time (in seconds) between when a user calls that "startgame" command and when the game actually starts. This defaults at 60 to ensure that players have enough time to get into the channel before the game begins.

### Creating Name Sets
*This section has been deprecated since the implementation of the 'createnameset' command. However, it remains for posterity.*

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
