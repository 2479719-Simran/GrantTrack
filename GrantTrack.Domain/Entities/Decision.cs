using System;

namespace GrantTrack.Domain.Entities;

public class Decision
{
    public int DecisionId { get; set; } 
    
    public string DecisionValue { get; set; } 

    public string? Notes { get; set; }

    public DateTime Date { get; set; } 

    public User? User { get; set; } 
    public int UserId { get; set; }

    public Application Application { get; set; } 
    public int ApplicationID { get; set; } 

    


}
