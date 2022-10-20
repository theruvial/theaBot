//using Discord.Commands;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theaBot.Services;

namespace theaBot.cogs
{
    public class Moderation : InteractionModuleBase<SocketInteractionContext>
    {       
        private readonly CommandHandler _handler;
        private readonly DiscordSocketClient _client;        
        private readonly IConfiguration _config;
        

        public Moderation (CommandHandler handler, DiscordSocketClient client, IConfiguration config)
        {
            _handler = handler;
            _client = client;            
            _config = config;
        }


        [SlashCommand("ping", "Pong!")]
        public async Task Ping()
        {
            await RespondAsync("Pong!");
        }

        [SlashCommand("kill", "Debug command that kills bot process")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KillBot()
        {
            var user = _handler.Context.User;
            await RespondAsync($"Killing Bot for {user.Mention}");
            Process.GetCurrentProcess().Kill();

        }

        [SlashCommand("kick", "kicks a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user)
        {            
            await user.KickAsync().ConfigureAwait(false);
            await RespondAsync($"Kicked user {user.Mention}");
        }

        [SlashCommand("status", "Sets the status of the bot")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task BotStatus(string botStatus)
        {
            await _client.SetGameAsync(botStatus);
        }

        [SlashCommand("echo", "Echoes the user of the command")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Echo(string userMessage)
        {
            await RespondAsync(userMessage);
        }

        [SlashCommand("purge", "Deletes the amount of messages specified by the user")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            ITextChannel textChannel = (ITextChannel)_handler.Context.Channel;
            var messages = await textChannel.GetMessagesAsync(amount).FlattenAsync();
            await textChannel.DeleteMessagesAsync(messages);
            await RespondAsync($"Deleted {amount} messages");            
        }

    }
}
