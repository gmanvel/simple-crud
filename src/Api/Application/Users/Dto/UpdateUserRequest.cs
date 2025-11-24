using System.ComponentModel.DataAnnotations;

namespace Api.Application.Users.Dto;

public class UpdateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Email { get; set; } = string.Empty;
}
