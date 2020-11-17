# [Kaguya](https://top.gg/bot/538910393918160916)
## An all-in-one Discord bot solution!

The Kaguya Discord Bot provides the pinnacle of what a general-purpose Discord bot can offer. Recently rewritten from the ground up, Kaguya strikes a perfect balance between ease of use and power.

## Developing and Contributing:
Currently, the best ways to contribute to the project are to [submit issues](https://github.com/stageosu/Kaguya/issues/new/choose) and [pull requests](https://github.com/stageosu/Kaguya/compare).

If you desire to fix a currently existing issue, please comment on that issue and let me know so I don't conflict with your development. Contributing to issues marked as Low Priority are especially helpful!

### System Configuration
If you wish to modify the codebase, please make sure you have met the following prerequisites:
- A desktop platform with the [.NET 5 SDK](https://dotnet.microsoft.com/download) installed.
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (Version 8.0.21 or later)
- An IDE capable of intellisense and which can support .NET 5 and C# 9. This project is primarily developed with [Jetbrains Rider](https://www.jetbrains.com/rider/nextversion/) (EAP 2020.3+ required), but [Microsoft's Visual Studio](https://visualstudio.microsoft.com/downloads/) and [Visual Studio Code](https://visualstudio.microsoft.com/downloads/) work as well.

### Downloading the source code
Clone the repository:
```
cd (your directory)
git clone https://github.com/stageosu/Kaguya.git
cd Kaguya
```

Updating the repository:
- For development builds, look for the latest version branch. This is *not* the 'development' branch.
    - ```
      git checkout (branch)
      git pull
      ```
- For stable releases, use the master branch.
    - ``` 
      git checkout master
      git pull  
      ```
### Building

Before building, if you desire to debug or test music functionality, you need to run the `Lavalink.jar` located inside `KaguyaProjectV2/LavalinkServer`. You may also need to have Port 2333 whitelisted in your firewall.

In order for Kaguya to run, you must configure a MySQL Database. Create one, then navigate to `KaguyaProjectV2/Resources/script.sql` and paste that into a SQL command prompt or editor, then execute it.

Ensure you have a bot to test with. Create one [here](https://discord.com/developers/applications).

Finally, navigate to `KaguyaProjectV2/Resources` again and open `config.json`. You will find that all of its values are empty or null. Below is an example of how you should configure this file. Everything must be wrapped in quotes except for the `BotOwnerId` and `LogLevelNumber` properties.

*config.json*
```
{
  "Token": "", // Your bot's token
  "BotOwnerId": 0, // Your Discord ID
  "LogLevelNumber": 0, // 0 = Trace, 1 = Debug, 2 = Info, 3 = Warning, 4 = Error.
  "DefaultPrefix": "$", // Default symbol bot looks for when executing command.
  "OsuApiKey": "", // Required for osu commands. https://github.com/ppy/osu-api/wiki for more info. Otherwise, leave blank.
  "TopGgApiKey": "", // Leave this blank
  "MySqlUsername": "", // The username for your database, e.g. root.
  "MySqlPassword": "", // The password for your database.
  "MySqlServer": "", // The address at which this database lives. e.g. localhost:3306
  "MySqlSchema": "", // The name of the schema you made that contains the tables.
  "TwitchClientId": "", // Really not used, will be removed at a later date. Leave blank.
  "TwitchAuthToken": "", // Leave blank
  "DanbooruUsername": "", // Required for NSFW features. (18+) Create an account at https://danbooru.donmai.us/users/new
  "DanbooruApiKey": "", // Required for NSFW features. (18+) Found at the bottom of your account information page at https://danbooru.donmai.us/profile
  "TopGgWebhookPort": 6969 // Must be at least 1000. Should be left alone. Not used in debugging.
}
```

If you have everything properly configured, you should be good to go. If the bot runs without errors and still doesn't respond to commands, *ensure your database is exactly identical* to the structure in the provided .sql script. Otherwise, feel free to write to me in my [support Discord](https://discord.gg/aumCJhr) and I will be happy to assist you.

## License
- Kaguya's code is intentionally **unlicensed**, unlike many open-source projects. Anyone who wishes to use or modify the code in this repository must adhere to these terms:
    - You may not reproduce, redistribute, or rebrand, for profit or otherwise, any part of the project.
    - Any contributors willfully release any copyright permission they otherwise would gain from contributing to this project and are not inherently entitled to anything if they choose to contribute.
    - Users may, and are encouraged to, clone, debug, and modify the code to their liking, so long as the sole intent of these actions is to improve this primary repository through a pull request.
      If you're just curious and want to look around the code, this is fine too.
    - Wanton disregard for or violation of these terms are subject to DMCA takedown notices.
    - If you have any questions about these terms, please email me at hburnett777@gmail.com.
- *TL;DR - Don't run off with the code and use it to run or improve your own bot.*

## Special Thanks
- [CakeAndBanana](https://github.com/CakeAndBanana) for v2.0 ORM implementations and core implementation of the new database, license for the Windows 2019 Datacenter software, and for many other code contributions and consultations during the early development phase.
- [BitMasher](https://github.com/BitMasher) for guiding me on the development of the KaguyaApi portion of the project. This allows users to vote on top.gg and receive an instant notification with rewards by responding to a webhook. This wouldn't be possible without his help.

## Main Featues
* Anti-Raid features to protect your server!
    - Protects your server from mass raids by allowing Administrators to configure whether to action a 
    certain amount of users who join within a specified time limit.
    - Example: `$antiraid 5 20 ban` - Bans 5 (or more) users if they join within 20 seconds of eachother.
* **FREE** Level Up Role Rewards
    - Allows Server Administrators to configure a role to be given when a user reaches a certain server level.
* Powerful logging features.
    - Kaguya can log:
    	- User banned/unbanned
        - User joined/left
        - Filtered phrase detected
        - User connected to/disconnected from/changed voice channel
        - Server/global/fishing level announcements
        - Antiraid events with who was actioned
        - Greetings
    - and MORE!
* Powerful moderation features.
    - Kaguya can:
        - Take action on a user who has been warned more than once.
        - Word and Phrase filtering
        - Create / Delete / Assign roles all without having to go into server settings. Just one command and you can make a new role.
        - Delete Unused Roles.
        - HyperBans: Permanently bans a user from this server and from **any other server that the command executor is an Administrator in.** *Kaguya must be present in all mutual servers for this to work properly.* (Use this command with caution)
        - Shadow bans
    - and more!
* Super fun and addicting fishing game, as well as other games!
* Fun economy system (With global and server specific levels.)
* Easy to remember command names and aliases.
* Extensive customization. (Don't like a command? Just toggle it off!)
* Customizable welcome messages
* High quality music commands.
* osu! integrations and features (including link parsing)
* Auto-assigned roles
* Give rep to users
* Praise servers
* NSFW Commands
* Rewards for upvoting Kaguya on [Top.gg.](https://top.gg/bot/538910393918160916/vote)
* Reaction Roles

## [Kaguya Premium](https://sellix.io/KaguyaStore) (Monthly Subscription):
The standard Kaguya Premium option unlocks many features for both individuals and their servers. Any server-wide premium benefits, marked as **(SW)** below, apply to all servers you own and have Kaguya in. If you want to redeem multiple Kaguya Premium keys, the time will stack for every server you redeem the key in, as well as the personal benefits will stack on your account. Please note that redeeming 3 separate 30 day keys will grant you and all of your servers 90 days of premium benefits, but will grant the individual servers 30 days of benefits each.

One-time currency infusions described below (such as point bonuses) will stack if multiple keys were purchased. Benefits such as bonus luck or increased gambling limit do not stack.

All perks below will last until your time as a premium subscriber runs out.

* Bet many more points than usual on gambling games.
* 25,000 points for every 30 days of premium time purchased.
* 2x points and exp from $daily
* 2x points and exp from $vote
* Save 25% on the cost of $fish-ing
* Special $profile badge
* $fish 3x as frequently
* More lenient rate limit (able to use commands more frequently than other users).
* Access to $doujin
* Access to $react
* Access to $weekly
* Access to $serverstats
* Access to $hyperban
* Access to $deleteunusedroles
* Access to $soundcloud, $twitchaudio
* Bonus luck on all gambling commands (including $fish)
* Store up to 1,000 fish bait instead of 100
* Deleted messages logged via "$log DeletedMessages" will now include archives of the message's content, deleted images, and attachments. **(SW)**
* Updated messages logged via "$log UpdatedMessages" will now include archives of the previous message's content, as well as the new message's content. **(SW)**
* "$log FilteredPhrases" will now include archives of the user's full message as well as the phrase they had their message removed for. **(SW)**
* Access to the following LogTypes: Mute, Unmute, Shadowban, Unshadowban, Warn, and Unwarn. Access this through the $logs command. **(SW)**
* Unlimited role rewards **(SW)**
* View more of a user's warn history via $unwarn **(SW)**
* Unlimited song duration (compared to 10 minutes) **(SW)**
* Unlimited music queue size **(SW)**
* Unlimited + enhanced $nsfw usage **(SW)**

### Purchase Kaguya Premium at the [Kaguya Store](https://sellix.io/KaguyaStore) for only $4.99 a month!

## Support
Join the [Kaguya Support Server](https://discord.gg/aumCJhr) and ask our support staff if you need help!


##### [Original V2 changes](https://github.com/stageosu/Kaguya/blob/master/v2.md)
