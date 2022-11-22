// See https://aka.ms/new-console-template for more information
internal static class Queries
{
    public static string SelectAll = $"select id as {nameof(SimpleDataModel.Id)}, raw_data as {nameof(SimpleDataModel.RawData)}, random_value as {nameof(SimpleDataModel.RandomValue)} from v_simple_data;";

    public static string InsertRaw(string raw, int random) => $"insert into simple_data (raw_data, random_value) values('{raw}', {random})";
    public static string InsertRawNoShards(string raw, int random) => $"insert into simple_data_no_shards (raw_data, random_value) values('{raw}', {random})";

    internal static string ReadByRandomValue(int v) => @$"
        select 
            id as {nameof(SimpleDataModel.Id)}, 
            raw_data as {nameof(SimpleDataModel.RawData)}, 
            random_value as {nameof(SimpleDataModel.RandomValue)} 
        from v_simple_data
        where random_value = {v}";

    internal static string ReadByRandomValueNoShards(int v) => @$"
        select 
            id as {nameof(SimpleDataModel.Id)}, 
            raw_data as {nameof(SimpleDataModel.RawData)}, 
            random_value as {nameof(SimpleDataModel.RandomValue)} 
        from simple_data_no_shards
        where random_value = {v}";

    public const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam aliquet lacinia urna. Sed vitae magna at nibh dignissim pretium. Proin fringilla eros convallis quam finibus elementum. Pellentesque condimentum ornare porta. Morbi condimentum velit augue, sit amet rutrum erat vehicula in. Aliquam aliquam, ante non varius tristique, odio augue tincidunt libero, sit amet lacinia purus tortor et purus. Mauris vel erat varius, placerat erat vitae, molestie ante. Donec sed porta nisl, eget lobortis justo. Nam sem velit, auctor at tellus vitae, tempus dapibus ligula.";
}