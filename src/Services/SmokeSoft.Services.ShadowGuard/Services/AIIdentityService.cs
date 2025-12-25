using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class AIIdentityService : IAIIdentityService
{
    private readonly ShadowGuardDbContext _context;

    public AIIdentityService(ShadowGuardDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AIIdentityDto>>> GetUserAIIdentitiesAsync(
        Guid userId, 
        int pageNumber = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.AIIdentities
            .Where(ai => ai.UserId == userId)
            .OrderByDescending(ai => ai.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(ai => ai.VoiceRecordings)
            .Select(ai => MapToDto(ai))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<AIIdentityDto>(items, totalCount, pageNumber, pageSize);

        return Result<PagedResult<AIIdentityDto>>.Success(pagedResult);
    }

    public async Task<Result<AIIdentityDto>> GetAIIdentityByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var aiIdentity = await _context.AIIdentities
            .Include(ai => ai.VoiceRecordings)
            .FirstOrDefaultAsync(ai => ai.Id == id && ai.UserId == userId, cancellationToken);

        if (aiIdentity == null)
        {
            return Result<AIIdentityDto>.Failure(
                "AI Identity not found",
                ErrorCodes.AI_IDENTITY_NOT_FOUND
            );
        }

        return Result<AIIdentityDto>.Success(MapToDto(aiIdentity));
    }

    public async Task<Result<AIIdentityDto>> CreateAIIdentityAsync(
        Guid userId, 
        CreateAIIdentityRequest request, 
        CancellationToken cancellationToken = default)
    {
        var aiIdentity = new AIIdentity
        {
            UserId = userId,
            Name = request.Name,
            GreetingStyle = request.GreetingStyle,
            Catchphrases = request.Catchphrases, // Already a string
            Formality = request.Formality,
            Emotion = request.Emotion,
            Verbosity = request.Verbosity,
            ExpertiseArea = request.ExpertiseArea,
            SensitivityLevel = request.SensitivityLevel,
            IsActive = true
        };

        _context.AIIdentities.Add(aiIdentity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AIIdentityDto>.Success(MapToDto(aiIdentity));
    }

    public async Task<Result<AIIdentityDto>> UpdateAIIdentityAsync(
        Guid id, 
        Guid userId, 
        UpdateAIIdentityRequest request, 
        CancellationToken cancellationToken = default)
    {
        var aiIdentity = await _context.AIIdentities
            .Include(ai => ai.VoiceRecordings)
            .FirstOrDefaultAsync(ai => ai.Id == id && ai.UserId == userId, cancellationToken);

        if (aiIdentity == null)
        {
            return Result<AIIdentityDto>.Failure(
                "AI Identity not found",
                ErrorCodes.AI_IDENTITY_NOT_FOUND
            );
        }

        aiIdentity.Name = request.Name;
        aiIdentity.GreetingStyle = request.GreetingStyle;
        aiIdentity.Catchphrases = request.Catchphrases; // Already a string
        aiIdentity.Formality = request.Formality;
        aiIdentity.Emotion = request.Emotion;
        aiIdentity.Verbosity = request.Verbosity;
        aiIdentity.ExpertiseArea = request.ExpertiseArea;
        aiIdentity.SensitivityLevel = request.SensitivityLevel;
        aiIdentity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AIIdentityDto>.Success(MapToDto(aiIdentity));
    }

    public async Task<Result> DeleteAIIdentityAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var aiIdentity = await _context.AIIdentities
            .FirstOrDefaultAsync(ai => ai.Id == id && ai.UserId == userId, cancellationToken);

        if (aiIdentity == null)
        {
            return Result.Failure(
                "AI Identity not found",
                ErrorCodes.AI_IDENTITY_NOT_FOUND
            );
        }

        // Soft delete
        aiIdentity.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AIIdentityDto MapToDto(AIIdentity aiIdentity)
    {
        return new AIIdentityDto
        {
            Id = aiIdentity.Id,
            Name = aiIdentity.Name,
            GreetingStyle = aiIdentity.GreetingStyle,
            Catchphrases = aiIdentity.Catchphrases, // Already JSON string
            Formality = aiIdentity.Formality,
            Emotion = aiIdentity.Emotion,
            Verbosity = aiIdentity.Verbosity,
            ExpertiseArea = aiIdentity.ExpertiseArea,
            SensitivityLevel = aiIdentity.SensitivityLevel,
            IsActive = aiIdentity.IsActive,
            CreatedAt = aiIdentity.CreatedAt,
            UpdatedAt = aiIdentity.UpdatedAt ?? aiIdentity.CreatedAt
        };
    }
}
