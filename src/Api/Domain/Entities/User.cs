using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Email { get; set; } = string.Empty;
}
