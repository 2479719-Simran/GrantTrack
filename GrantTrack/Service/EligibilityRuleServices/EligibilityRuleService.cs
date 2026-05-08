using System;
using GrantTrack.Dto.EligibilityRulesDtos;
using GrantTrack.Repository.EligibilityRuleRepositories;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Utility;

namespace GrantTrack.Service.EligibilityRuleServices;

public class EligibilityRuleService : IEligibilityRuleService
{
    private readonly IEligibilityRuleRepository ruleRepository;
    private readonly IProgramRepository programRepository;

    public EligibilityRuleService(
        IEligibilityRuleRepository ruleRepository,
        IProgramRepository programRepository)
    {
        this.ruleRepository = ruleRepository;
        this.programRepository = programRepository;
    }

    public async Task<CreateEligibilityRuleResponseDto> CreateRule(CreateEligibilityRuleRequestDto request)
    {
        // Validate request
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.RuleDescription))
            throw new ArgumentException("Rule description is required.");

        if (string.IsNullOrWhiteSpace(request.RuleExpression))
            throw new ArgumentException("Rule expression is required."); 
        IExpressionValidator expressionValidator = new ExpressionSyntaxValidator();  
        SyntaxCheckResult val = expressionValidator.Validate(request.RuleExpression);
        if(!val.IsValid)
            throw new ArgumentException(val.Error);
        
        // Ensure the program exists before attaching a rule to it
        var programExists = await programRepository.ExistsAsync(request.ProgramId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {request.ProgramId} does not exist.");

        // Ensure the program is active — rules cannot be added to inactive programs
        var programActive = await programRepository.IsActiveAsync(request.ProgramId);
        if (!programActive)
            throw new InvalidOperationException($"Program with ID {request.ProgramId} is inactive. Cannot add rules.");

        return await ruleRepository.CreateRule(request);
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRules()
    {
        return await ruleRepository.GetRules();
    }

    public async Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRulesByProgramId(int programId)
    {
        if (programId <= 0)
            throw new ArgumentException("Program ID must be greater than zero.");

        var programExists = await programRepository.ExistsAsync(programId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {programId} does not exist.");

        return await ruleRepository.GetRulesByProgramId(programId);
    }

    public async Task<UpdateEligibilityRuleResponseDto> UpdateRule(int ruleId, UpdateEligibilityRuleRequestDto request)
    {
        if (ruleId <= 0)
            throw new ArgumentException("Rule ID must be greater than zero.");

        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.RuleDescription))
            throw new ArgumentException("Rule description is required.");

        if (string.IsNullOrWhiteSpace(request.RuleExpression))
            throw new ArgumentException("Rule expression is required.");

        // Ensure the rule exists
        var ruleExists = await ruleRepository.ContainsRuleId(ruleId);
        if (!ruleExists)
            throw new KeyNotFoundException($"Eligibility rule with ID {ruleId} does not exist.");

        IExpressionValidator expressionValidator = new ExpressionSyntaxValidator();  
        SyntaxCheckResult val = expressionValidator.Validate(request.RuleExpression);
        if(!val.IsValid)
            throw new ArgumentException(val.Error);
        // Ensure the (possibly reassigned) program exists and is active
        var programExists = await programRepository.ExistsAsync(request.ProgramId);
        if (!programExists)
            throw new KeyNotFoundException($"Program with ID {request.ProgramId} does not exist.");

        var programActive = await programRepository.IsActiveAsync(request.ProgramId);
        if (!programActive)
            throw new InvalidOperationException($"Program with ID {request.ProgramId} is inactive. Cannot update rules.");

        return await ruleRepository.UpdateRule(ruleId, request);
    }

    public async Task<DeleteEligibilityRuleResponseDto> DeleteRule(int ruleId)
    {
        if (ruleId <= 0)
            throw new ArgumentException("Rule ID must be greater than zero.");

        var ruleExists = await ruleRepository.ContainsRuleId(ruleId);
        if (!ruleExists)
            throw new KeyNotFoundException($"Eligibility rule with ID {ruleId} does not exist.");

        return await ruleRepository.DeleteRule(ruleId);
    }

    
}
