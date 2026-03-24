namespace Common.DTOs;

public class UpdateProfileDto
{
    public string Email { get; set; } = "";
    public string? NewPassword { get; set; }
}
