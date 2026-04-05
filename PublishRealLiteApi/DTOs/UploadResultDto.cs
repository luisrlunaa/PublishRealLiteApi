namespace PublishRealLiteApi.DTOs
{
    public class UploadResultDto
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
