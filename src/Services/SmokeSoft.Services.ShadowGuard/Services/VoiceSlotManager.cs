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
    
    public VoiceSlotManager(
        ShadowGuardDbContext context,
        ISystemConfigService configService)
    {
        _context = context;
        _configService = configService;
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
                // TODO: Call ElevenLabs API to delete voice
                // await _elevenLabsService.DeleteVoiceAsync(lruSlot.ElevenLabsVoiceId);
                
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
        
        // TODO: Clone voice from sample
        // var newVoiceId = await _elevenLabsService.CloneVoiceFromSampleAsync(
        //     voiceSample.BlobUrl, 
        //     aiIdentity.Name);
        
        // For now, generate a mock voice ID
        var newVoiceId = $"voice_{Guid.NewGuid():N}";
        
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
        
        // TODO: Call ElevenLabs API to delete voice
        // await _elevenLabsService.DeleteVoiceAsync(slot.ElevenLabsVoiceId);
        
        slot.IsActive = false;
        slot.DeletedFromElevenLabsAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}
