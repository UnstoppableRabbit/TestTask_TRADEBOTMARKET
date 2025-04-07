public class ScraperJobConfig
{
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
    public int Hour { get; set; }
    public int Minute { get; set; }
}