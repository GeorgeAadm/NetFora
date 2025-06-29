namespace NetFora.Application.DTOs.Requests
{
    public class ModerationRequest
    {
        public int Flags { get; set; }
        public string? Reason { get; set; }
    }
}
