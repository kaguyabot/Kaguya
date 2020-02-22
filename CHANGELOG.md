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