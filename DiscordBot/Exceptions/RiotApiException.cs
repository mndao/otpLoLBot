using System;
namespace DiscordBot.Exceptions
{
    public class RiotApiException : Exception
    {
        public RiotApiException()
        {
        }

        public RiotApiException(string message)
            :base(message)
        {
        }

        public RiotApiException(string message, Exception inner)
            :base(message,inner)
        {
        }
    }
}
