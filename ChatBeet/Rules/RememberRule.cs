﻿using ChatBeet.Data;
using ChatBeet.Data.Entities;
using ChatBeet.Utilities;
using GravyBot;
using GravyIrc.Messages;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatBeet.Rules
{
    public class RememberRule : AsyncMessageRuleBase<PrivateMessage>
    {
        private readonly MemoryCellContext ctx;
        private readonly IrcBotConfiguration config;
        private readonly Regex rgx;

        public RememberRule(MemoryCellContext ctx, IOptions<IrcBotConfiguration> options)
        {
            config = options.Value;
            this.ctx = ctx;
            rgx = new Regex($"^({Regex.Escape(config.Nick)}, |{Regex.Escape(config.CommandPrefix)})remember (.*?)=(.*)", RegexOptions.IgnoreCase);
        }

        public override bool Matches(PrivateMessage incomingMessage) => rgx.IsMatch(incomingMessage.Message);

        public override async IAsyncEnumerable<IClientMessage> RespondAsync(PrivateMessage incomingMessage)
        {
            var match = rgx.Match(incomingMessage.Message);
            if (match.Success)
            {
                var key = match.Groups[2].Value.Trim();
                var value = match.Groups[3].Value.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    yield return new PrivateMessage(
                            incomingMessage.GetResponseTarget(),
                            $"{incomingMessage.From}: provide a name to define."
                        );
                }
                else if (string.IsNullOrEmpty(value))
                {
                    yield return new PrivateMessage(
                            incomingMessage.GetResponseTarget(),
                            $"{incomingMessage.From}: provide a value to set for {IrcValues.BOLD}{key}{IrcValues.RESET}."
                        );
                }
                else
                {
                    var existingCell = await ctx.MemoryCells.FirstOrDefaultAsync(c => c.Key.ToLower() == key.ToLower());
                    if (existingCell != null)
                    {
                        ctx.MemoryCells.Remove(existingCell);
                        await ctx.SaveChangesAsync();
                    }

                    ctx.MemoryCells.Add(new MemoryCell
                    {
                        Author = incomingMessage.From,
                        Key = key,
                        Value = value
                    });
                    await ctx.SaveChangesAsync();

                    yield return new PrivateMessage(incomingMessage.GetResponseTarget(), "Got it! 👍");

                    if (existingCell != null)
                    {
                        yield return new PrivateMessage(
                            incomingMessage.GetResponseTarget(),
                            $"Previous value was {IrcValues.BOLD}{existingCell.Value}{IrcValues.RESET}, set by {existingCell.Author}."
                        );
                    }
                }
            }
        }
    }
}