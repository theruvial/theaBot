using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using theaBot.cogs;

namespace theaBot
{
    public class Bot
    {
        
        public DiscordClient Client { get; private set; }
        public SlashCommandsExtension Slash { get; private set; }
        
        
        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            //Console.WriteLine(configJson.Token);


            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };            

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;


            var commandsConfig = new SlashCommandsConfiguration();
            
            

            //Commands = Client.UseCommandsNext(commandsConfig);
            Slash = Client.UseSlashCommands(commandsConfig);

            Slash.RegisterCommands<Moderation>(454792201633792001) ;
            
            //Commands.RefreshCommands();
            

            await Client.ConnectAsync();
                       
            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            Console.WriteLine($"Registered Class Count: {Slash.RegisteredCommands.Count}");            
            return Task.CompletedTask;
        }
    }
}
