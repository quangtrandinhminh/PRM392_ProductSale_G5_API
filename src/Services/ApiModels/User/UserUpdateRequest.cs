using Services.Constants;
using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.User;

public class UserUpdateRequest
{
    [Required]
    public int UserId { get; set; }
    [Required(ErrorMessage = ResponseMessageIdentity.USERNAME_REQUIRED)]
    [MaxLength(100)]
    public string Username { get; set; }
    public string? Address { get; set; }
}