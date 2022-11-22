// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Dapper;
using Npgsql;

await WriteTestAsync();

ReadTest();

static void ReadTest()
{
    var connection = new NpgsqlConnection(GetConnectionString());
    connection.Open();
    var res = new long[1000];
    for (int i = 0; i < 1000; i++)
    {
        var sw = Stopwatch.StartNew();
        connection.Query<SimpleDataModel>(Queries.ReadByRandomValueNoShards(new Random().Next(300000)));
        sw.Stop();

        res[i] = sw.ElapsedMilliseconds;
    }
    Console.WriteLine($"Avg read time: {res.Average()}ms");
}

static async Task WriteTestAsync()
{
    var connString = GetConnectionString();

    var sw = Stopwatch.StartNew();
    await Parallel.ForEachAsync(Enumerable.Range(1, 4), new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (i, _) =>
    {
        var connection = new NpgsqlConnection(connString);
        connection.Open();

        for (int ii = 0; ii < 250000; ii++)
        {
            await connection.ExecuteAsync(Queries.InsertRawNoShards(Queries.LoremIpsum, new Random().Next(300000)));
        }
    });

    sw.Stop();

    var connection = new NpgsqlConnection(connString);
    connection.Open();
    Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
    Console.WriteLine($"Total raws: {connection.QueryFirst<int>("select count(id) from simple_data_no_shards;")}");
}

static string GetConnectionString()
{
    return new NpgsqlConnectionStringBuilder()
    {
        Database = "ShardingDb",
        Host = "localhost",
        Port = 5432,
        Username = "postgre",
        Password = "password",
        Timeout = 1024
    }.ToString();
}