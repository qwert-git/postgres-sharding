// See https://aka.ms/new-console-template for more information
record SimpleDataModel
{
    public int Id { get; set; }

    public string RawData { get; set; } = string.Empty;

    public int RandomValue { get; set; }
}