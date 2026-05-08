using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.EligibilityRulesDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper; 

namespace GrantTrack.Repository.EligibilityRuleRepositories;

public class EligibilityRuleRepository : IEligibilityRuleRepository
{
    private readonly GrantTrackDbContext dbContext;
    private readonly IMapper mapper;

    public EligibilityRuleRepository(GrantTrackDbContext dbContext , IMapper mapper)
    {
        this.dbContext = dbContext; 
        this.mapper = mapper; 

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

        return mapper.Map<CreateEligibilityRuleResponseDto>(newRule); 
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRules()
    {
        var rules = await dbContext.EligibilityRules.ToListAsync();

        return mapper.Map<IEnumerable<GetEligibilityRuleResponseDto>>(rules); 
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRulesByProgramId(int programId)
    {
        List<GetEligibilityRuleResponseDto> response = new List<GetEligibilityRuleResponseDto>();
        var rules = await dbContext.EligibilityRules
                                   .Where(q => q.ProgramId == programId)
                                   .ToListAsync();

        return mapper.Map<IEnumerable<GetEligibilityRuleResponseDto>>(rules);
    }

    public async Task<UpdateEligibilityRuleResponseDto> UpdateRule(int ruleId, UpdateEligibilityRuleRequestDto request)
    {
        var obj = await dbContext.EligibilityRules.FindAsync(ruleId);

        obj.ProgramId = request.ProgramId;
        obj.RuleDescription = request.RuleDescription;
        obj.RuleExpression = request.RuleExpression;

        await dbContext.SaveChangesAsync();

        return mapper.Map<UpdateEligibilityRuleResponseDto>(obj);
    }

    public async Task<DeleteEligibilityRuleResponseDto> DeleteRule(int ruleId)
    {
        var obj = await dbContext.EligibilityRules.FindAsync(ruleId);

        dbContext.EligibilityRules.Remove(obj);
        await dbContext.SaveChangesAsync();

        return mapper.Map<DeleteEligibilityRuleResponseDto>(obj);
    }
}
