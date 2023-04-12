using Newtonsoft.Json;
using Shared.Network;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public class RegisterRequest : LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^((?!\.)[\w-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "RePassword is required.")]
        public string RePassword { get; set; }

        [JsonIgnore]
        public string Role { get; set; } = Constants.BasicRole;
    }
}