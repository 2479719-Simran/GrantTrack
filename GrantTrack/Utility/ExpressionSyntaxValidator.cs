using System;
using System.Text.RegularExpressions;
using GrantTrack.Dto.EligibilityRulesDtos;

namespace GrantTrack.Utility;

public class ExpressionSyntaxValidator : IExpressionValidator
{   
    private static readonly HashSet<string> AllowedIdentifiers = new (StringComparer.OrdinalIgnoreCase)
    {
         "applicant.annualIncome"
        , "applicant.location", "applicant.employeeCount"
    };

    private static readonly HashSet<string> AllowedOperators = new()
    {
        "==", "!=", "<", "<=", ">", ">=", "&&", "||", "!"
    }; 

    private static Regex SafeCharacters =  new(@"^[\w\.\s<>=!&|()""'\d,]+$", RegexOptions.Compiled);
    public SyntaxCheckResult Validate(string expression)
    {
        if(string.IsNullOrWhiteSpace(expression)) 
            return new SyntaxCheckResult(false , "Expression contains invalid characters. Only identifiers, operators, and literals are allowed."); 
        if (!SafeCharacters.IsMatch(expression))
            return new SyntaxCheckResult(false,
                "Expression contains invalid characters. Only identifiers, operators, and literals are allowed.");
        if(!CheckBalancedParentheses(expression)) 
            return new SyntaxCheckResult(false, "Expression has unbalanced parentheses.");

        var usedIdentifiers = ExtractIdentifiers(expression);  
        if (!usedIdentifiers.Any())
        return new SyntaxCheckResult(false,
            $"Expression must contain at least one valid identifier. " +
            $"Allowed: {string.Join(", ", AllowedIdentifiers)}");
        var unknownIdentifiers = usedIdentifiers.Except(AllowedIdentifiers ,StringComparer.OrdinalIgnoreCase).ToList(); 
        if(unknownIdentifiers.Any()) 
            return new SyntaxCheckResult(false,
                $"Unknown identifier(s): {string.Join(", ", unknownIdentifiers)}. " +
                $"Allowed: {string.Join(", ", AllowedIdentifiers)}");

        return new SyntaxCheckResult(true , null); 
    }   


    private static bool CheckBalancedParentheses(string expr)
    {
        int depth = 0;
        foreach (char c in expr)
        {
            if (c == '(') depth++;
            else if (c == ')') depth--;
            if (depth < 0) return false;
        }
        return depth == 0;
    } 

    private static IEnumerable<string> ExtractIdentifiers(string expr)
    {
        var matches = Regex.Matches(expr , @"\b[a-zA-Z_]\w*\.[a-zA-Z_]\w*\b");
        return matches.Select(m => m.Value).Distinct(StringComparer.OrdinalIgnoreCase); 
    }
}
