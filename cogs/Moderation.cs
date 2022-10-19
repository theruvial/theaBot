using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace theaBot.cogs
{
    public class Moderation : ApplicationCommandModule
    {     

        [SlashCommand("Ping", "Basic Test Command")]
        public async Task PingPong(InteractionContext ctx)
        {
            await ctx.Channel.SendMessageAsync("FUCK");
        }

    }
}
