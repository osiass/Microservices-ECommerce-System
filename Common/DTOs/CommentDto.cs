namespace Common.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
