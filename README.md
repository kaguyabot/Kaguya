# Kaguya: Version 2.0 
#### *A complete rewrite!*

## Core Changes:

- Data storage overhaul: From JSON to MySQL. *Kaguya Version 1 was using a single JSON file to store data for over 85,000 users. 
This resulted in absurd amounts of overwrites that would periodically make Kaguya crash. I never intended for Kaguya to grow as large 
as it has today, so this change was absolutely necessary and was the core reason for the rewrite.*
- Cleaner, more organized code all around. *I have learned quite a lot since my initial viewing of a "C# Discord Bot Tutorial" video. My knowledge is certainly reflected in the codebase, and I will continue to improve things overtime where I can.*
- All commands are now signinifcantly more intuitive and functional than V1.
- Arguments may now be passed when launching the program instead of relying on a JSON "config" file. If no arguments are passed, the config file will be used as an alternative way to retreive credentials.
- Kaguya is now as reliable as ever and is many times more efficient than V1.
- `$help <cmd>` now displays the command's information much more clearly.
- `Owner Only` commands are now only displayed for bot owners.
- Some commands have been renamed and will not use the same names or aliases as V1. However, most commands do share the same aliases or names as V1 for consistency.

## Command Changes: Administration

- `$antiraid` no longer has the intuitive setup that V1 has. This way changes to the anti-raid config may be made on the fly.
- `$antiraidoff` has been **removed.** Using `$antiraid` by itself will disable it.
- `$channelblacklist [cbl]` has been completely reworked (it's awesome).
- `$channelunblacklist [cubl]`, `$channelwhitelist [cwl]`, and `$channelunwhitelist [cuwl]` have been **removed.**
- `$clear [c] [purge]` may now clear up to `1,000` messages (was 100).
- `$createrole [cr]` may now create a list of roles, separated by periods.
- `$filterclear [clearfilter]` has been replaced with `$filterremoveall [fra]`
- `$inspect` has been **removed.**
- `$kaguyaexit` has been **removed.**
- `$kick` and `$ban` may now action multiple users at once.
- `$masskick` and `$massban` have been removed.
- `$mute [m]` will now mute users immediately, even if the `kaguya-mute` role does not exist when using the command.
- Music commands no longer start with `$m`.
- `$removeallroles [rar]` may now target multiple users.
- `$warnoptions [wo]` has been **removed**.
- `$warnset [ws]` has been renamed to `$warnsettings [warnset] [ws]`
- `$warnpunishments [wp]` has been removed. `$warnset` used without any arguments displays the server's warn-punishment scheme.

## Other Command Changes:

- New server-specific `$praise` feature added. This is essentially `$rep` but for servers. There is more functionality with this that server administrators have control over, such as the cooldown.
- **New game: `$fish`!** Collect fish and hunt for the **Legendary Big Kahuna!** Fish may be caught, collected, and sold for KP and **global** EXP. Dedicated fishermen may level up their fishing skill by fishing often, and bonuses can be earned for fishing a certain amount of times per day! The higher your fishing level, the more likely you are to catch 
rarer fish! Be wary though, as better bait will be needed to catch rarer fish more easily, which will be more expensive!
- `Critical hits` removed from all games.
- `$tictactoe` removed (it was terrible anyway)
- `$bugreport` will now simply reply with a link to the bug report Github form.
- `$masspointsdistribute` has been removed.
- `$redeem` will now support keys of all types (Kaguya Supporter, Kaguya Premium, etc.) instead of there being separate `$redeem` commands for each key type.
- `$nsfw bomb` command removed. **Note:** Although this individual command is removed, using `$nsfw bomb` by itself will still trigger the same function as V1. It is just not present in the command list.

## Quality of life Changes:

- 1-4 points are now earned automatically for typing in chat at the same time you earn exp. *Only being able to earn points through betting and "timely" claims was unfun. Now, points can be earned passively so that you can buy fish bait, bet, etc. whenever it's convenient!*
- NSFW age verification requirement has been removed. *It's implied that anyone with access to a NSFW-marked channel is already of age.*
- All upvote rewards are now automatically given soon after you upvote online (top.gg, search for Kaguya).
- A small amount of EXP is now given for votes.
- If you haven't recently used NSFW commands (within the last 7 days), you won't be reminded that your NSFW image cooldown reset has triggered when voting.

## Kaguya Supporter (Monthly Subscription):
#### Supporter Only Features
*Kaguya Supporter is a user-bound monthly subscription that grants access to special commands and features.*
- Unlimited `$nsfw` command uses (compared to 12 images per day).
- `$nsfw` may now have tags appended to it (`$nsfw long_hair swimsuit`). Tag count is unlimited.
- Significantly more lenient rate limit when using commands.
- Higher priority suggestions for new features.
- Special **Supporter** role in the Kaguya Support Discord Server.
- Special badge on your `$profile`
- Access to supporter only commands, including `$react`.
- Reduced fishing cooldown (15s => 5s).
- 25% off fishing bait purchase price.
- Increased maximum betting range for games (50,000 => 500,000)
- Access to $soundcloud music streaming in any server shared with Kaguya.
- Access to $twitchaudio music streaming in any server shared with Kaguya.
- Unlimited duration on songs (compared to 10 minutes).

## Kaguya Premium (Monthly Subscription):
#### Kaguya Premium Features
*Kaguya Premium is a server-bound monthly subscription that grants access to many special utilities. The following features are restricted to premium servers only.*
- Access to the `logtype` `ModLog`. This logtype will automatically log events for `AutoBan`, `AutoKick`, `AutoMute`, `AutoShadowban`, `Shadowban`, `Unshadowban`, `Mute`, `Unmute`, `Warn`, `UnWarn`, `Bulk deletion of messages ($clear 100)`.
- If a message is deleted and the `DeletedMessages` logtype is enabled, the log will now display **permanent links** to any attachments or files that were uploaded and included with the deleted message, 
including images and anything else that a user may upload to the server.
- View the **9** most recent warnings for users when using the $unwarn command (instead of the 4 most recent). This is especially useful if you want to remove a warning that was made a long time ago, or just view the user's warn history.
- Ability to link up to 15 Twitch channels for live notifications (instead of 3).
- Access to `$serverstats` command which displays many stats about your server.
- Server-wide access to $soundcloud music searching.
- Server-wide access to $twitchaudio audio streaming.
- Server-wide unlimited duration on songs (compared to 10 minutes).
- Unlimited song queue size (compared to 50 songs).