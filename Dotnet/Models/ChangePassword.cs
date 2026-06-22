using System.ComponentModel.DataAnnotations;

namespace Dotnet.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "Please Enter your new password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please Enter your current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Please Enter your confirm password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string EmailToken { get; set; }

        public string EmailAddress { get; set; }
    }
}
