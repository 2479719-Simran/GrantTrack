using System;
using GrantTrack.Dto.EligibilityRulesDtos;

namespace GrantTrack.Utility;

public interface IExpressionValidator
{
    SyntaxCheckResult Validate(string Expression); 
}
