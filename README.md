# Kaguya: Version 2.0 
#### *A complete rewrite!*

## Core Changes:

- Data storage overhaul: From JSON to MySQL. *Comments: Kaguya Version 1 was using a single JSON file to store data for over 85,000 users. 
This resulted in absurd amounts of overwrites that would periodically make Kaguya crash. I never intended for Kaguya to grow as large 
as it has today, so this change was absolutely necessary and was the core reason for the rewrite.*
- Cleaner, more organized code all around. *Comments: I have learned quite a lot since my initial viewing of a "C# Discord Bot Tutorial" video. My knowledge is certainly reflected in the codebase, and I will continue to improve things overtime where I can.*
- All commands are now signinifcantly more intuitive and functional than V1.
- Arguments may now be passed when launching the program instead of relying on a JSON "config" file. If no arguments are passed, the config file will be used as an alternative way to retreive credentials.
- Kaguya is now as reliable as ever and is many times more efficient than V1.
- $help <cmd> now displays the command's information much more clearly.
- `Owner Only` commands are now only displayed for bot owners.

## Command Changes: Administration

- `$assignrole [ar]` has been renamed to `$addrole [ar]`
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
- **New game: `$fish`!** Collect fish and hunt for the **Legendary Big Kahuna!** Fish may be caught, collected, and sold.
- `Critical Hits` removed from all games.
- `$tictactoe` removed (it was terrible anyway)
