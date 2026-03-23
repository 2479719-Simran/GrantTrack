using System;

namespace GrantTrack.Domain.Entities;

public class GrantTrackException:Exception
{
    public GrantTrackException(string err):base(err)
    {
        
    }
}
