using Microsoft.EntityFrameworkCore;

namespace GitGudModsListLoader.Persistence
{
    public enum Role
    {
        Anonymous = 0,
        Admin = 128
    }

    [Index(nameof(ExternalId), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        public required int ExternalId { get; set; }

        public Role Role { get; set; }
    }
}
