using System;
using StackExchange.Redis;

namespace RedisDbAndReplicationUsage
{
    /* Пример использования репликации redis  
     * ( для примера был настроен кластер в докере портами 7000 для мастера и 7001 для реплики
     */
    class Program
    {
        private static ConfigurationOptions options = new ConfigurationOptions();

        private const string RedisConnectionString = "localhost:7000"; // хост мастера
        private const string RedisReplicaConnectionString = "localhost:7001"; // хост реплики

        private static ConnectionMultiplexer connection =
            ConnectionMultiplexer.Connect(RedisConnectionString);
        private static ConnectionMultiplexer connectionReplica =
            ConnectionMultiplexer.Connect(RedisReplicaConnectionString);
        static void Main()
        {
            while (true)
            {
                Console.WriteLine("Enter amount of generating records for redis db ('q' to exit):");
                var input = Console.ReadLine();
                if (input == "q") return;

                int amount = 0;
                if (int.TryParse(input, out amount) && amount > 0)
                {
                    TestDbReplica(amount);
                }
            }
        }

        private  static void TestDbReplica(int testStringAmount)
        {
            var originalDb = connection.GetDatabase();
            Console.WriteLine($"Got connection to database #{originalDb.Database} | host: {RedisConnectionString}");
            var replicaDb = connectionReplica.GetDatabase();
            Console.WriteLine($"Got connection to replica database #{originalDb.Database} | host: {RedisReplicaConnectionString}");
            for (int i = 0; i < testStringAmount; i++)
            {
                originalDb.StringSet(originalDb.Database + ":" + i, i.ToString());
            }
            Console.WriteLine($"Write {testStringAmount} strings to original db");

            int isFailings = -1;
            for (int i = 0; i < testStringAmount; i++)
            {
                var gotValue = replicaDb.StringGet(originalDb.Database + ":" + i);
                if (gotValue != i.ToString())
                {
                    isFailings = i;
                    Console.WriteLine($"Error! Expected value: {i}, got value: {gotValue}");
                }
                else
                {
                    Console.WriteLine($"expected value: {i}, got value: {gotValue}");
                }

                Console.WriteLine(isFailings != -1 ? $"Has errors on iteration {isFailings}" : "Successful complited");
            }
        }

    }
}