using System.ComponentModel.DataAnnotations;

namespace Dotnet.Models
{
    public class userEdit
    {
        public short UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public string UserPhoto { get; set; } = null!;

        public string UserRole { get; set; } = null!;

        public string UserPassword { get; set; } = null!;

        public string CurrentAddress { get; set; } = null!;

        public string EncId { get; set;} = null!;

        [DataType(DataType.Upload)]

        public IFormFile? UserFile { get; set; } = null;

        public String EmailToken { get; set; } = null!;

        public Boolean IsEmailVerified { get; set; }
    }
}
