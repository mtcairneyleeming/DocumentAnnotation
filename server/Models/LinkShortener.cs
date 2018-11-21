namespace DocumentAnnotation.Models
{
    public class LinkShortener
    {
        public int LinkShortenerId { get; set; }
        public string ShortLink { get; set; }
        public string OriginalLink { get; set; }
    }
}