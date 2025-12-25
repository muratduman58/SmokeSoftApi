using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IVoiceSlotManager
{
    Task<Result<string>> EnsureVoiceSlotAsync(AIIdentity aiIdentity);
    Task<Result> DeleteVoiceSlotAsync(Guid aiIdentityId);
}

public class VoiceSlotManager : IVoiceSlotManager
{
    private readonly ShadowGuardDbContext _context;
    private readonly ISystemConfigService _configService;
    private readonly IElevenLabsVoiceService _elevenLabsService;
    private readonly ILogger<VoiceSlotManager> _logger;
    
    public VoiceSlotManager(
        ShadowGuardDbContext context,
        ISystemConfigService configService,
        IElevenLabsVoiceService elevenLabsService,
        ILogger<VoiceSlotManager> logger)
    {
        _context = context;
        _configService = configService;
        _elevenLabsService = elevenLabsService;
        _logger = logger;
    }
    
    public async Task<Result<string>> EnsureVoiceSlotAsync(AIIdentity aiIdentity)
    {
        // Check if slot already exists
        var existingSlot = await _context.VoiceSlots
            .FirstOrDefaultAsync(s => s.AIIdentityId == aiIdentity.Id && s.IsActive);
        
        if (existingSlot != null)
        {
            // Update last used time
            existingSlot.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result<string>.Success(existingSlot.ElevenLabsVoiceId);
        }
        
        // Check slot limit
        var config = await _configService.GetConfigAsync();
        var activeSlots = await _context.VoiceSlots.CountAsync(s => s.IsActive);
        
        if (activeSlots >= config.AbsoluteMaxVoiceSlots)
        {
            // LRU: Delete least recently used
            var lruSlot = await _context.VoiceSlots
                .Where(s => s.IsActive)
                .OrderBy(s => s.LastUsedAt)
                .FirstOrDefaultAsync();
            
            if (lruSlot != null)
            {
                try
                {
                    await _elevenLabsService.DeleteVoiceAsync(lruSlot.ElevenLabsVoiceId);
                    _logger.LogInformation("Deleted LRU voice slot: {VoiceId}", lruSlot.ElevenLabsVoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete voice from ElevenLabs: {VoiceId}", lruSlot.ElevenLabsVoiceId);
                }
                
                lruSlot.IsActive = false;
                lruSlot.DeletedFromElevenLabsAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        
        // Get voice sample
        var voiceSample = await _context.VoiceSamples
            .FirstOrDefaultAsync(vs => vs.AIIdentityId == aiIdentity.Id);
        
        if (voiceSample == null)
        {
            return Result<string>.Failure("Voice sample not found for this AI identity");
        }
        
        // Clone voice from sample
        string newVoiceId;
        try
        {
            // TODO: Download audio from Azure Blob Storage using voiceSample.BlobUrl
            // For now, we'll need to implement blob download separately
            // This is a placeholder that assumes we have the audio stream
            using var audioStream = new MemoryStream(); // Placeholder - should download from BlobUrl
            newVoiceId = await _elevenLabsService.CloneVoiceAsync(aiIdentity.Name, audioStream);
            _logger.LogInformation("Cloned voice for AI Identity {Name}: {VoiceId}", aiIdentity.Name, newVoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone voice for AI Identity {Name}", aiIdentity.Name);
            return Result<string>.Failure($"Failed to clone voice: {ex.Message}");
        }
        
        // Create new slot
        var newSlot = new VoiceSlot
        {
            AIIdentityId = aiIdentity.Id,
            ElevenLabsVoiceId = newVoiceId,
            LastUsedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        _context.VoiceSlots.Add(newSlot);
        await _context.SaveChangesAsync();
        
        return Result<string>.Success(newVoiceId);
    }
    
    public async Task<Result> DeleteVoiceSlotAsync(Guid aiIdentityId)
    {
        var slot = await _context.VoiceSlots
            .FirstOrDefaultAsync(s => s.AIIdentityId == aiIdentityId && s.IsActive);
        
        if (slot == null)
        {
            return Result.Success(); // Already deleted
        }
        
        try
        {
            await _elevenLabsService.DeleteVoiceAsync(slot.ElevenLabsVoiceId);
            _logger.LogInformation("Deleted voice slot: {VoiceId}", slot.ElevenLabsVoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete voice from ElevenLabs: {VoiceId}", slot.ElevenLabsVoiceId);
        }
        
        slot.IsActive = false;
        slot.DeletedFromElevenLabsAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}
