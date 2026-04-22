using System;
namespace GrantTrack.Domain.Entities;

public class Document
{
    public int DocumentId { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public string DocType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileURI { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UploadedAt { get; set; }
}