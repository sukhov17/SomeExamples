using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using StackExchange.Redis;

namespace tryRedis2
{
    class Program
    {
        private static ConfigurationOptions options = new ConfigurationOptions();

        private const string RedisConnectionString = "localhost:7000";

        private static ConnectionMultiplexer connection =
            ConnectionMultiplexer.Connect(RedisConnectionString);

        private static string ChatChannel = "Chat-Simple-Channel"; // Can be anything we want.
        private static string userName = string.Empty;

        static void Main()
        {

        }

        public static int ToChannels()
        {
            // connection.PreserveAsyncOrder = false; // разрешает ассинхронную работу
            // Вводим имя для отображения в чате
            Console.Write("Enter your name: ");
            userName = Console.ReadLine();

            // Вводим имя канала
            Console.Write("Enter channel: ");
            ChatChannel = Console.ReadLine();

            // Создаем pub/sub
            var pubsub = connection.GetSubscriber();

            // Подписываемся на канал
            pubsub.Subscribe(ChatChannel, (channel, message) => MessageAction(message));

            // Говорим всем подписчикам канала (каналов), что мы присоединились
            pubsub.Publish(ChatChannel, $"'{userName}' joined the chat room.");

            // Messaging here
            while (true)
            {
                string nMessage = Console.ReadLine();

                #region Команды для информации о клиенте

                if (nMessage != null &&
                    (nMessage.Contains(" -!e") || nMessage.Contains("-!e ") || nMessage == "-!e")) return -1;
                if (nMessage != null && (nMessage.Contains(" -e") || nMessage.Contains("-e ") || nMessage == "-e"))
                {
                    return 0;
                }

                if (nMessage != null && nMessage.Contains("-mul"))
                {
                    nMessage += Environment.NewLine + "multiplexer cli name: " + pubsub.Multiplexer.ClientName +
                                Environment.NewLine
                                + "multiplexer config: " + pubsub.Multiplexer.Configuration +
                                pubsub.Multiplexer.IsConnected;

                }

                if (nMessage != null && nMessage.Contains("-cli"))
                {
                    nMessage += Environment.NewLine + "pubsub.IdentifyEndpoint: " +
                                pubsub.IdentifyEndpoint(ChatChannel).ToString();

                }

                #endregion

                pubsub.PublishAsync(ChatChannel, $"{userName}:" + nMessage, CommandFlags.None);
            }
        }

        // действие при получении сообщения
        private static void MessageAction(RedisValue message)
        {
            Console.WriteLine(message);
        }

    }
}