using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Nest;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theaBot.Services;

namespace theaBot.cogs
{
    public class Points : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CommandHandler _handler;
        private readonly IConfiguration _config;
        private NpgsqlConnection _dbConn = default!;
        private readonly DiscordSocketClient _bot;
        private readonly string connString;


        public Points(CommandHandler handler, IConfiguration config, DiscordSocketClient bot)
        {
            _handler = handler;            
            _config = config;            
            _bot = bot;
            _bot.MessageReceived += OnMessage;
            connString = $"Host={_config["dbHost"]};Username={_config["dbUser"]};Password={_config["dbPass"]};Database={_config["dbName"]}";
        }        

        // Listening for each time a message is sent in any channel
        public async Task OnMessage(SocketMessage message)
        {

            var discordId = message.Author.Id;
            if (discordId == _bot.CurrentUser.Id) return;
            //Opening DataBase
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();

            string sql = $"SELECT max(ds) FROM exp WHERE discord_id = {discordId} AND TYPE = 'msg' LIMIT 1";
            await using var cmd = new NpgsqlCommand(sql, _dbConn);
            NpgsqlDataReader result = await cmd.ExecuteReaderAsync();            
            while (result.Read())
            {
                long lastExpDs = (long)result[0];
                if (lastExpDs == 0) lastExpDs = 0;
                if(Time() >= lastExpDs + 60)
                {
                    await AddPoints(message.Author, 1, "msg");
                    Console.WriteLine("Added 1 Points");
                }
            }            
            await _dbConn.CloseAsync();
        }        

        public async Task AddPoints(SocketUser member, int points, string typeValue)
        {
            //Opening the Database
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();
            
            long ds = Time();
            int pts = points;
            string sql_exp = $"INSERT INTO exp(ds,discord_id,exp,type) VALUES({ds},{member.Id},{pts},'{typeValue}')";
            string sql_bucks = $"INSERT INTO bucks(ds,discord_id,bucks,type) VALUES({ds},{member.Id},{pts},'{typeValue}')";
            await using var batch = new NpgsqlBatch(_dbConn)
            {
                BatchCommands =
                {
                    new(sql_exp),
                    new(sql_bucks)
                }
            };
            await batch.ExecuteNonQueryAsync();
            await _dbConn.CloseAsync();
        }

        public async Task AddBucks(SocketUser member, int points, string typeValue)
        {
            //Opening the Database
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();

            long ds = Time();
            int pts = points;
            string sql_bucks = $"INSERT INTO bucks(ds,discord_id,bucks,type) VALUES({ds},{member.Id},{pts},'{typeValue}')";
            await using var cmd = new NpgsqlCommand(sql_bucks, _dbConn);
            await cmd.ExecuteNonQueryAsync();
            await _dbConn.CloseAsync();
        }


        [SlashCommand("exp", "Gets the user's current Exp and Bucks Value")]
        public async Task GetExp()
        {
            //Opening the Database
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();
            
            //Setting up our local variables
            SocketUser member = (SocketUser)_handler.Context.User;
            long discordId = (long)member.Id;
            string sql_exp = $"SELECT SUM(exp.exp) FROM exp WHERE discord_id = {discordId}";                        
            
            //Executing our Sql query
            await using var cmdExp = new NpgsqlCommand(sql_exp, _dbConn);
            NpgsqlDataReader result = await cmdExp.ExecuteReaderAsync();
            
            while (result.Read())
            {
                //Console.WriteLine(result[0]);
                Console.WriteLine($"EXP: {result[0]}");
                var expEmbed = new EmbedBuilder
                {
                    Title = $"Current EXP for {member.Mention}:",
                    Description = result[0].ToString(),
                    Color = Color.DarkPurple
                };
                
                await RespondAsync(embed: expEmbed.Build());
                await _dbConn.CloseAsync();
            }            
        }

        [SlashCommand("bucks", "Gets current bucks for user")]
        public async Task GetBucks()
        {
            //Opening the Database
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();

            //Setting up our local variables
            SocketUser member = (SocketUser)_handler.Context.User;
            long discordId = (long)member.Id;
            string sql_bucks = $"SELECT SUM(bucks.bucks) FROM bucks WHERE discord_id = {discordId}";

            //Executing our Sql query
            await using var cmdBucks = new NpgsqlCommand(sql_bucks, _dbConn);
            NpgsqlDataReader result = await cmdBucks.ExecuteReaderAsync();

            //Reading Sql query
            while (result.Read())
            {
                //Console.WriteLine(resultBucks[0]);
                Console.WriteLine($"BUCKS: {result[0]}");
                var bucksEmbed = new EmbedBuilder
                {
                    Title = $"Current BUCKS for {member.Username}: ",
                    Description = result[0].ToString(),
                    Color = Color.DarkPurple
                };
                await RespondAsync(embed: bucksEmbed.Build());
                await _dbConn.CloseAsync();
            }
        }

        [SlashCommand("award", "Rewards the user with x amount of points")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AwardPoints(SocketUser member, int points)
        {
            SocketUser author = (SocketUser)_handler.Context.User;
            await AddBucks(member, points, "award");
            var embed = new EmbedBuilder
            {
                Title = $"{author.Username} has rewarded {member.Username} {points} points!"
            };
            await RespondAsync(embed: embed.Build());
        }       

        //Helper function that returns current time in seconds
        public static long Time()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long currentTime = (long)t.TotalSeconds;
            return currentTime;
        }      
    }
}
