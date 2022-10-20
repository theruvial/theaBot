using Discord.Interactions;
using Microsoft.Extensions.Configuration;
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
        private CommandHandler _handler;
        private IConfiguration _config;
        public NpgsqlConnection DbConn { get; private set; }

        public Points(CommandHandler handler)
        {
            _handler = handler;
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");
            _config = builder.Build();
            ConnectDB().GetAwaiter().GetResult();
        }

        public async Task ConnectDB()
        {
            //Connecting to Database hosted on raspberry pi and creating a cursor
            var connString = $"Host={_config["dbHost"]};Username={_config["dbUser"]};Password={_config["dbPass"]};Database={_config["dbName"]}";
            await using var conn = new NpgsqlConnection(connString);
            DbConn = conn;
            await DbConn.OpenAsync().ConfigureAwait(false);
            Console.WriteLine("Successfully connected to Database!");
        }




    }
}
