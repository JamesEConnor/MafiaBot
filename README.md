# MafiaBot
A bot submitted for Discord's Hackweek. It runs a game of Mafia and can be customized with different "name sets" that are in JSON format. It's delimeter is "/m"

### Commands
```
startgame     [name set] - Creates a game of Mafia, as well as any necessary channels or roles.
set           [setting]  - Modifies a setting for the server.
```

### Settings
* Max Players [maxplayers] - The maximum number of players allowed in a game of Mafia. Any value less than or equal to 0 is the same as none. The default is -1.
* Minimum Players [minplayers] - The minimum number of players required to start a game. This must be at least 5, and defaults to that value.
* Custom name sets allowed. [namesets] - Determines whether or not custom name sets are allowed to be used. This can only be set by the server owner.
* Time to Start [timetostart] - How much time (in seconds) between when a user calls that "startgame" command and when the game actually starts. This defaults at 60 to ensure that players have enough time to get into the channel before the game begins.

### Creating Name Sets
