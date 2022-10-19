using System;
using DSharpPlus;
using Newtonsoft.Json;
using theaBot;
using theaBot.cogs;

public class Program
{
    
    public static void Main(string[] args)
    {
        var bot = new Bot();
        bot.RunAsync().GetAwaiter().GetResult();        
    }
}