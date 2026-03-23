using System;

namespace GrantTrack.Domain.Entities;

public class Document
{
    public int DocumentId { get; set; }    public Application Application { get; set; }
    public int ApplicantionId { get; set; }
    public string DocType { get; set; }
    public string FileURI { get; set; }
}
