using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket; 

namespace DiscordBot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService commands;
        private readonly DiscordSocketClient discord;
        private readonly IServiceProvider services;

        public CommandHandlingService(IServiceProvider services)
        {
            commands = services.GetRequiredService<CommandService>();
            discord = services.GetRequiredService<DiscordSocketClient>();
            this.services = services;

            commands.CommandExecuted += CommandExceutedAsync;

            discord.MessageReceived += MessageReceivedAsync; 

        }

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(),services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;

            if (!message.HasMentionPrefix(discord.CurrentUser, ref argPos)) return;

            //if (!message.HasCharPrefix('!', ref argPos)) return; 

            var context = new SocketCommandContext(discord, message);

            await commands.ExecuteAsync(context, argPos, services);
        }

        public async Task CommandExceutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return;

            if (result.IsSuccess) return;

            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}
