namespace PublishRealLiteApi.DTOs
{
    public class StreamStatDto
    {
        public DateTime Date { get; set; }
        public string Platform { get; set; }
        public string Country { get; set; }
        public int Streams { get; set; }
        public string MetricType { get; set; }
        public string Source { get; set; }
    }

}
