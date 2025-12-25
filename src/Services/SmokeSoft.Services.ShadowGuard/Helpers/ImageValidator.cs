namespace SmokeSoft.Services.ShadowGuard.Helpers;

public static class ImageValidator
{
    // Magic bytes for common image formats
    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        {
            "image/jpeg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG EXIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, // JPEG raw
            }
        },
        {
            "image/png", new[]
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        },
        {
            "image/webp", new[]
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF (WebP starts with RIFF)
            }
        }
    };

    public static async Task<bool> IsValidImageAsync(Stream stream, string contentType)
    {
        if (stream == null || !stream.CanRead)
            return false;

        // Normalize content type
        contentType = contentType.ToLower().Trim();
        if (contentType == "image/jpg")
            contentType = "image/jpeg";

        if (!MagicBytes.ContainsKey(contentType))
            return false;

        // Read first 12 bytes (enough for all image formats)
        var buffer = new byte[12];
        var originalPosition = stream.Position;
        
        try
        {
            stream.Position = 0;
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            
            if (bytesRead < 4)
                return false;

            // Check against magic bytes for the content type
            var magicBytesList = MagicBytes[contentType];
            
            foreach (var magicBytes in magicBytesList)
            {
                if (BytesMatch(buffer, magicBytes))
                {
                    // Special check for WebP (needs to verify WEBP signature at offset 8)
                    if (contentType == "image/webp")
                    {
                        if (bytesRead >= 12)
                        {
                            var webpSignature = new byte[] { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"
                            if (BytesMatch(buffer, webpSignature, 8))
                                return true;
                        }
                        continue;
                    }
                    
                    return true;
                }
            }

            return false;
        }
        finally
        {
            // Reset stream position
            stream.Position = originalPosition;
        }
    }

    private static bool BytesMatch(byte[] buffer, byte[] magicBytes, int offset = 0)
    {
        if (buffer.Length < offset + magicBytes.Length)
            return false;

        for (int i = 0; i < magicBytes.Length; i++)
        {
            if (buffer[offset + i] != magicBytes[i])
                return false;
        }

        return true;
    }

    public static string GetSupportedFormatsMessage()
    {
        return "Supported formats: JPEG, PNG, WebP";
    }
}
