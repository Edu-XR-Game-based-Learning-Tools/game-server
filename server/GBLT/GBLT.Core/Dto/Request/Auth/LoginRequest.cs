using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core.Dto
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        //[RegularExpression(@"^[A-Za-z]\w{5, 29}$", ErrorMessage = "Invalid username.")]
        public string UserName { get; set; }
        //[RegularExpression(@"^^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,32}$",
        //            ErrorMessage = "Password must follow:\n" +
        //    "At least one uppercase, lowercase, digit, and specific character.\n" +
        //    "Length must be in range 8-32")]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [JsonIgnore]
        public string RemoteIpAddress { get; set; }
    }
}