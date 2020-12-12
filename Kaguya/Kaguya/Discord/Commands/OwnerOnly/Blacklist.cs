﻿using System;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    // TODO: Test if this owneronly actually blocks execution from non owners.
    [Module(CommandModule.OwnerOnly)]
    [Group("blacklist")]
    public class Blacklist : KaguyaBase<Blacklist>
    {
        private readonly ILogger<Blacklist> _logger;
        private readonly BlacklistedEntityRepository _blacklistedEntityRepository;

        public Blacklist(ILogger<Blacklist> logger, BlacklistedEntityRepository blacklistedEntityRepository) : base(logger)
        {
            _logger = logger;
            _blacklistedEntityRepository = blacklistedEntityRepository;
        }

        [Command("-add")]
        [Summary("Blacklists the entity based on ID for an optional duration.\n" +
                 "Valid entity types are `channel`, `user`, and `server`.")]
        [Remarks("<entity id> <entity type> [duration] [reason]")] // Delete if no remarks needed.
        public async Task BlacklistEntityCommand(ulong id, string type, string duration = null, [Remainder]string reason = null)
        {
            BlacklistedEntity entity = await _blacklistedEntityRepository.GetAsync(id);
            if (entity != null)
            {
                await SendBasicErrorEmbedAsync("This entity is already blacklisted.\n\n" +
                                               $"ID: {entity.EntityId.ToString().AsBold()}\n" +
                                               $"Type: {entity.EntityType.Humanize(LetterCasing.Title).AsBold()}\n" +
                                               $"Reason: {entity.Reason ?? "<No reason>".AsBold()}\n" +
                                               $"Expiration: {entity.ExpirationTime?.Humanize(false).AsBold() ?? "Permanent".AsBold()}");
            }
            else
            {
                DateTime? expiration = null;
                if (duration != null)
                {
                    var parser = new TimeParser(duration);
                    var parsedDuration = parser.ParseTime();
                    if (parsedDuration != TimeSpan.Zero)
                    {
                        expiration = DateTime.Now.Add(parsedDuration);
                    }
                }
                if (Enum.TryParse(type, true, out BlacklistedEntityType entityType))
                {
                    var newEntity = new BlacklistedEntity
                    {
                        EntityId = id,
                        EntityType = entityType,
                        ExpirationTime = expiration,
                        Reason = reason
                    };
                    
                    await _blacklistedEntityRepository.InsertAsync(newEntity);

                    await SendBasicSuccessEmbedAsync($"Blacklisted entity with ID {newEntity.EntityId.ToString().AsBold()}.");
                }
                else
                {
                    await SendBasicErrorEmbedAsync("Could not parse entity type.");
                }
            }
        }

        [Command("-undo")]
        [Alias("-remove", "-u")]
        public async Task UnblacklistCommand(ulong id)
        {
            var match = await _blacklistedEntityRepository.GetAsync(id);
            if (match == null || match.HasExpired)
            {
                await SendBasicErrorEmbedAsync("This entity is not blacklisted.");

                return;
            }

            match.ExpirationTime = DateTime.Now;
            await _blacklistedEntityRepository.UpdateAsync(match);
            
            await SendBasicSuccessEmbedAsync($"Unblacklisted entity with ID {id.ToString().AsBold()}");
        }
    }
}