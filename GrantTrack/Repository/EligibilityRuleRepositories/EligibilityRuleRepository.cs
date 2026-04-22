using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.EligibilityRulesDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.EligibilityRuleRepositories;

public class EligibilityRuleRepository : IEligibilityRuleRepository
{
    private readonly GrantTrackDbContext dbContext;

    public EligibilityRuleRepository(GrantTrackDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> ContainsProgramId(int programId)
    {
        var obj = await dbContext.EligibilityRules.FirstOrDefaultAsync(q => q.ProgramId == programId);

        if (obj == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<bool> ContainsRuleId(int ruleId)
    {
        var obj = await dbContext.EligibilityRules.FindAsync(ruleId);

        if (obj == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<CreateEligibilityRuleResponseDto> CreateRule(CreateEligibilityRuleRequestDto request)
    {
        var newRule = new EligibilityRule
        {
            ProgramId = request.ProgramId,
            RuleDescription = request.RuleDescription,
            RuleExpression = request.RuleExpression
        };

        dbContext.Add(newRule);
        await dbContext.SaveChangesAsync();

        return new CreateEligibilityRuleResponseDto
        {
            RuleId = newRule.RuleId,
            ProgramId = newRule.ProgramId,
            RuleDescription = newRule.RuleDescription,
            RuleExpression = newRule.RuleExpression
        };
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRules()
    {
        List<GetEligibilityRuleResponseDto> response = new List<GetEligibilityRuleResponseDto>();
        var rules = await dbContext.EligibilityRules.ToListAsync();

        foreach (var rule in rules)
        {
            response.Add(new GetEligibilityRuleResponseDto
            {
                RuleId = rule.RuleId,
                ProgramId = rule.ProgramId,
                RuleDescription = rule.RuleDescription,
                RuleExpression = rule.RuleExpression
            });
        }

        return response;
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRulesByProgramId(int programId)
    {
        List<GetEligibilityRuleResponseDto> response = new List<GetEligibilityRuleResponseDto>();
        var rules = await dbContext.EligibilityRules
                                   .Where(q => q.ProgramId == programId)
                                   .ToListAsync();

        foreach (var rule in rules)
        {
            response.Add(new GetEligibilityRuleResponseDto
            {
                RuleId = rule.RuleId,
                ProgramId = rule.ProgramId,
                RuleDescription = rule.RuleDescription,
                RuleExpression = rule.RuleExpression
            });
        }

        return response;
    }

    public async Task<UpdateEligibilityRuleResponseDto> UpdateRule(int ruleId, UpdateEligibilityRuleRequestDto request)
    {
        var obj = await dbContext.EligibilityRules.FindAsync(ruleId);

        obj.ProgramId = request.ProgramId;
        obj.RuleDescription = request.RuleDescription;
        obj.RuleExpression = request.RuleExpression;

        await dbContext.SaveChangesAsync();

        return new UpdateEligibilityRuleResponseDto
        {
            
            ProgramId = obj.ProgramId,
            RuleDescription = obj.RuleDescription,
            RuleExpression = obj.RuleExpression
        };
    }

    public async Task<DeleteEligibilityRuleResponseDto> DeleteRule(int ruleId)
    {
        var obj = await dbContext.EligibilityRules.FindAsync(ruleId);

        dbContext.EligibilityRules.Remove(obj);
        await dbContext.SaveChangesAsync();

        return new DeleteEligibilityRuleResponseDto
        {
            RuleId = obj.RuleId,
            ProgramId = obj.ProgramId,
            RuleDescription = obj.RuleDescription,
            RuleExpression = obj.RuleExpression
        };
    }
}
