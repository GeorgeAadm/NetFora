namespace NetFora.Application.DTOs
{
    public class ModerationRequest
    {
        public int Flags { get; set; }
        public string? Reason { get; set; }
    }
}
