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