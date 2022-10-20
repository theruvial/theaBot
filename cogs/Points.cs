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
        

        //public async int CheckLastExpDs(int discordId, string typeValue)
        //{
        //    var sql = $"select max(ds) from exp where discord_id = {discordId} and type = '{typeValue}' Limit 1";
        //    await using var cmd = new NpgsqlCommand(sql, _dbConn);
        //    NpgsqlDataReader result = await cmd.ExecuteReaderAsync();
        //    if (result == null)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        return result[0];
        //    }


        //}

        public async Task OnMessage(SocketMessage message)
        {

            var discordId = message.Author.Id;
            if (discordId == _bot.CurrentUser.Id) return;
            //Opening DataBase
            await using var conn = new NpgsqlConnection(connString);
            _dbConn = conn;
            await _dbConn.OpenAsync();

            string sql = $"select max(ds) from exp where discord_id = {discordId} and type = 'msg' Limit 1";
            await using var cmd = new NpgsqlCommand(sql, _dbConn);
            NpgsqlDataReader dr = await cmd.ExecuteReaderAsync();            
            while (dr.Read())
            {
                long lastExpDs = (long)dr[0];
                if (lastExpDs == 0) lastExpDs = 0;
                if(Time() >= lastExpDs) // REMEMBER TO ADD BACK IN THE +60 
                {
                    await AddPoints(message.Author, 1, "msg");
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

        //Helper function that returns current time in seconds
        public static long Time()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long currentTime = (long)t.TotalSeconds;
            return currentTime;
        }


        




    }
}
