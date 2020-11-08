using System;
using System.IO;
using System.Threading.Tasks; 
using Discord;
using Discord.Commands;
using DiscordBot.Services; 

namespace DiscordBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public PictureService PictureService { get; set;}

        public RiotApiService RiotApiService { get; set;}
        

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong");

        [Command("GetRankedHistory")]
        public async Task GetRankedInfoAsync(params string[] objects)
        {
            if (objects.Length != 2)
            {
                await ReplyAsync("Too many arguments");
            }
            else
            {
                var result = await RiotApiService.GetRankedHistory(objects[0]);
                await ReplyAsync(result);
            }
        }

        [Command("cat")]
        public async Task CatAsync()
        {
            var stream = await PictureService.GetPictureAsync();
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "cat.png");
        }

        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("ban")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            await user.Guild.AddBanAsync(user, reason: reason);
            await ReplyAsync("okay");
        }

        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
            => ReplyAsync('\u200b' + text);

        [Command("list")]
        public Task ListAsync(params string[] objects)
            => ReplyAsync("You listed : "  + string.Join(";", objects));

        [Command("guild_only")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry this command must be ran from within a server, not a DM!")]
        public Task GuildOnlyCommand()
            => ReplyAsync("Nothing to see here"); 
   

    }
}
