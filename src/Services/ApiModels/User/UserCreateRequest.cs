using System.ComponentModel.DataAnnotations;
using Services.Constants;

namespace Services.ApiModels.User;

public class UserCreateRequest
{
    [Required(ErrorMessage = ResponseMessageIdentity.USERNAME_REQUIRED)]
    [MaxLength(100)]
    public string Username { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.EMAIL_REQUIRED)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.PHONENUMBER_REQUIRED)]
    [Phone(ErrorMessage = ResponseMessageIdentity.PHONENUMBER_INVALID)]
    [StringLength(10, MinimumLength = 10, ErrorMessage = ResponseMessageIdentity.PHONENUMBER_LENGTH)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.PASSWORD_REQUIRED)]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = ResponseMessageIdentity.PASSSWORD_LENGTH)]
    [MaxLength(100)]
    public string Password { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.CONFIRM_PASSWORD_REQUIRED)]
    [Compare(nameof(Password), ErrorMessage = ResponseMessageIdentity.PASSWORD_NOT_MATCH)]
    [DataType(DataType.Password)]
    [MaxLength(100)]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = ResponseMessageIdentity.ROLES_REQUIRED)]
    public int Role { get; set; }
}