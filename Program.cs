using System;
using Newtonsoft.Json;
using theaBot;
//using theaBot.cogs;

public class Program
{
    
    public static void Main(string[] args) => new Bot().RunAsync().GetAwaiter().GetResult();
    
}