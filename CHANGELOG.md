### Version 2.4.3
- Fixed a bug where Kaguya Premium users could not play the $dice game.

### Version 2.4.2
- $ban now only allows users to ban one user at a time, but now provides support for ban reasons.
- $kick now only allows users to kick one user at a time, but now provides support for kick reasons.
- Possibly fixed a bug where users who sold multiple fish at once wouldn't receive their payout. This *might* have been due to too many concurrent database connections being made at once. This patch attempts to fix this.

### Version 2.4.1
- Fixed a small text error with $dice.

### Version 2.4
- Axed "Kaguya Supporter" system - everything now falls under one subscription: "Kaguya Premium".
    - Kaguya Premium works the same as normal, but now all Kaguya Supporter perks will apply to the *redeemer* of the key.
    - Kaguya Premium is now the only thing able to be purchased from the online store.
- Fixed a bug with $nsfw bomb where it would break if tags were appended.
- Fixed a bug where users would not receive their Kaguya Premium role automatically.
- Fixed a bug where users who no longer have Kaguya Premium wouldn't have their role removed in the Kaguya Support discord server.
- All Kaguya Premium commands are now displayed in their respective command categories, rather than on a separate command page list. All premium commands will have a `{$}` appended to the end of them, after the aliases.
- Fixed a bug where if a user had multiple active Kaguya Premium keys, they would be DM'd as soon as any one of them expired. Now the user will only be notified if all other keys have run out and they are no longer a premium user.
- *Note: The benefits described below do not apply to the Kaguya Support server.*
- Kaguya Premium servers now grant +5% luck to all betting events.
- Kaguya Premium servers now grant +5% luck to all fishing events.
- Kaguya Premium servers now grant +50% bonus to all points and EXP received from $daily.
- Kaguya Premium key redeemers now have priority DM support (DM Stage with any questions or concerns!). Stage will not respond to DMs from non-premium users if related to Kaguya.
- Kaguya Premium key redeemers now receive +100% bonus EXP and points when using $upvote
- New command: $choose
- New command: $reverse
- New game: $dice

### Version 2.3.2
- Added lots of redundancy against crashes that occur in relation to being unable to send users direct messages. (This has gone on for too long!!)

### Version 2.3.1
- Fixed a fatal error where the bot would crash if an attempt was made to send a reminder to a user who no longer shared any guilds with the bot.

### Version 2.3
- **New command:** $myreminders [reminders][mr] -- Allows users to view and delete any existing reminders they have.
- Fixed a bug where users in the Kaguya Support discord server would not get their supporter/premium roles automatically added/removed to/from them when it was necessary.
- **New command:** $addquote [aq] -- Allows users with the manage messages permission to add quotes for their server!
- **New command:** $randomquote [rq] -- Allows users to view a random quote from the server, no permissions needed.
- **New command:** $removequote [deletequote] [dq] [rq] -- Allows users with the manage messages permission to remove quotes from the server.
- **New command:** $allquotes [listquotes] [quotes] -- Allows users with the manage messages permission to view all quotes for the current server.
- Reminders will now be sent in sentence casing, rather than all lower case letters.

### Version 2.2
- New command: $exp -- Allows users to quickly view their global and server exp and ranks.

### Version 2.1.4
- Fixed a bug where after mass-selling a specific fish type, an error message would be thrown stating that the reactions for the confirmation message have been disabled.

### Version 2.1.3
- Slightly tweaked appearance of $flb leaderboard to match the other leaderboard commands.

### Version 2.1.2
- Fixed bug with $fishleaderboard where "Unknown User" would be shown if the user wasn't in the server the command was executed in.

### Version 2.1.1
- Added even more unhandled exception logging (so solutions to problems can be found faster)
- Added a new command: $fishleaderboard [$flb]

### Version 2.1
- Removed April Fool's "uwu-ify'd" text.
- Fixed a bug where it was impossible to sell a fish by their type. Now if you want to sell a specific fish type, you can use `$sell <type>`. Example: `$sell giant sea bass` or `$sell small_salmon`. You can replace the underscores with spaces for the fish types.
- Upvoting on top.gg will now reward users with a constant 750 points and 500 exp. Was formerly...Points: Random between 150-700 | Exp: Random between 75-350.
- Fixed a bug where songs wouldn't automatically play after each other when queued.
- $ss "date created" value is now much more exact.
- **NEW COMMAND:** `$loop` -- Allows a user to repeat a song up to 10 times. Use $h loop for more info!

### Version 2.0.12
- Patched a bug where, when using $scg, if the greeting was already enabled for the server, the footer of the response embed would still show that the server had to use "$tg" in order to enable it.
- Other small improvements.

### Version 2.0.11
- Fixed a fatal error where, upon trying to automatically execute a warnaction after a user reaches the specified "warn threshold" (to auto kick, ban, mute, or shadowban), the bot would crash. This would occur if a guild didn't have Kaguya's permissions set properly. Now, the error will be logged but no action will be taken against the warned user.

### Version 2.0.10
- Fixed a fatal bug where Kaguya would repeatedly crash - this was because the antiraid service was trying to action users that it wasn't supposed to (because they weren't in the server anymore).

### Version 2.0.9
- Added ability for bot owner(s) to blacklist users.

### Version 2.0.8
- Patched an exploit that allowed users to sell the same fish (via ID) over and over.

### Version 2.0.7
- Cleaned up some console logging issues (users won't notice this)
- $daily now rewards a constant 750 points and 275 exp. (Was 35-700 points and 8-112 exp.)
- Fixed a bug where after using $unwarn, a unnecessary notification would be sent after 5 minutes.
- If any of the GIF commands are used without a target, it will now show that you "<action>ed the air!".
- Fixed a fatal bug where Kaguya would crash if it could not send ratelimit DMs to users.

### Version 2.0.6
- If the server's custom greeting is disabled when they set a new greeting, they will be given a clear message displaying that the message needs to be enabled via a separate command.
- Generic stats now logged to database.
- Kaguya will now DM owners with a brief message whenever she joins a new guild.

### Version 2.0.5
- Top.gg stats are now posted every 15 minutes (server and shard counts).
- `$remindme` time responses are now more precise.
- When catching a fish, the "you now have `x` fish" dialogue now accounts for whether you have previously sold fish, meaning it won't tell you you have a certain count of the fish if you've previously sold it.
- Made adjustments to fish level-up rewards. Here's what changed if you were fish level 15: 
    - `6.12% increased chance to catch a rare fish (was 4.54%)`
    - `31.30% base increased fish value (was 28.98%)`
    - `10.00% decreased tax penalty when selling a fish (was 7.50%)`
    - `117.10% increased bait cost (was 143.42%)`
    I made these changes as players would consistently lose a lot of points if they were fishing often. More balance changes will come if needed to these values and even the direct values of some fish, if needed.
- Giant squids and Big Kahunas will now reward many more points (about double) if caught.

### Version 2.0.4
- Got rid of TwitchNotifications logtype - it was accidentally left in the production build.

### Version 2.0.3
- NSFW images are now earned at a rate of 1 every 15 minutes for non-supporters (compared to 1 every 2 hours).

### Version 2.0.2
- Fixed a bug where users weren't getting points or exp for voting (this is separate, but related, to the notification bug that was patched in v2.0.1).
- Fixed a bug where the NSFW image handler timer would not auto reset.

### Version 2.0.1

- Fixed a bug where the $buybait command was displaying an incorrect maximum value for how much bait you could buy based on how many points you have.
- "Total value" renamed to "taxed value" on $myfish.
- Fixed a bug where, upon attempt to buy more bait than your baitbox can hold, the error message would not be thrown.
- Command added to view active Kaguya Supporter / Kaguya Premium subscriptions: $checkexpiration [ce]
- Fixed a bug where $nsfw bomb [tags] wouldn't check for whether the user was a Kaguya Supporter, resulting in a finite usage of the command.
- Fixed a bug where tagged $nsfw bomb usages would never send any images for any user.
- Fixed a bug where, if you had more than one Kaguya Supporter key active at a time, your subscription would essentially count down twice as fast.
- Fixed a bug where the thread for top.gg upvote notifications would break and never be restarted, resulting in lost rewards for voting.
- Fixed a bug where "Message Updated" logs would get sent for messages even if the content of the message (text) hadn't changed.
- Added $faq command.