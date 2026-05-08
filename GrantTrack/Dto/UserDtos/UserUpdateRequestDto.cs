using System;

namespace GrantTrack.Dto;

using System;



public class UpdateUserRequestDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool Status { get; set; }
    public string? Email { get; set; }  
    public int annualIncome { get; set; } 
    public string location {get; set;} 
    public int employeeCount { get; set; }
    

}

