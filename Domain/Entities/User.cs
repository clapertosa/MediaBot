namespace Domain.Entities
{
    public class User : Base<int>
    {
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public bool IsBot { get; set; }
        public int PublicFlags { get; set; }
    }
}