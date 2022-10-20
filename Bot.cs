using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using theaBot.Services;

namespace theaBot
{
    public class Bot
    {
        private DiscordSocketClient _bot;
        private InteractionService _commands;
        private ulong _testGuildId;
        private IConfiguration _config;
        
        public async Task RunAsync()
        {
            // Here we deserialize the config Json object
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // Here we assign some variables for later use
            _config = builder.Build();
            _testGuildId = ulong.Parse(_config["guildId"]);
                      
            using (var services = ConfigureServices())
            {
                var bot = services.GetRequiredService<DiscordSocketClient>();
                var commands = services.GetRequiredService<InteractionService>();
                _bot = bot;
                _commands = commands;
                
                // This is where we subscribe to events;
                _bot.Log += Log;
                _bot.UserJoined += AnnounceUserJoined;
                _bot.Ready += ReadyAsync;

                //Startup Bot
                await _bot.LoginAsync(TokenType.Bot, _config["token"]);
                await _bot.StartAsync();

                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                

                // Delay bot shutdown indefinitely so bot doesn't prematurely shutdown
                await Task.Delay(Timeout.Infinite);
            }
            
            

            
        }

        //Basic logging function
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        // Function to announce new user joining server
        private async Task AnnounceUserJoined(SocketGuildUser user)
        {
            var guild = user.Guild;
            var channel = guild.DefaultChannel;
            await channel.SendMessageAsync($"Welcome, {user.Mention}");
        }
        
        private async Task ReadyAsync()
        {
            if (IsDebug())
            {
                Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
                await _commands.RegisterCommandsToGuildAsync(_testGuildId);
            }
            else
            {
                await _commands.RegisterCommandsGloballyAsync(true);
            }
            Console.WriteLine($"Connected as -> [{_bot.CurrentUser}]");
        }

        private ServiceProvider ConfigureServices()
        {
            // this returns a service provider that is used later to call for those services
            // we can add types we have access to here
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        static bool IsDebug()
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }

        
    }
}
