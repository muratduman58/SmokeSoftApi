namespace SmokeSoft.Shared.DTOs.ShadowGuard;

public class CreateAIIdentityRequest
{
    public string Name { get; set; } = string.Empty;
    public string GreetingStyle { get; set; } = string.Empty;
    public string Catchphrases { get; set; } = string.Empty;
    public int Formality { get; set; }
    public int Emotion { get; set; }
    public int Verbosity { get; set; }
    public string ExpertiseArea { get; set; } = string.Empty;
    public int SensitivityLevel { get; set; }
}

public class UpdateAIIdentityRequest
{
    public string Name { get; set; } = string.Empty;
    public string GreetingStyle { get; set; } = string.Empty;
    public string Catchphrases { get; set; } = string.Empty;
    public int Formality { get; set; }
    public int Emotion { get; set; }
    public int Verbosity { get; set; }
    public string ExpertiseArea { get; set; } = string.Empty;
    public int SensitivityLevel { get; set; }
    public bool IsActive { get; set; }
}

public class AIIdentityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GreetingStyle { get; set; } = string.Empty;
    public string Catchphrases { get; set; } = string.Empty;
    public int Formality { get; set; }
    public int Emotion { get; set; }
    public int Verbosity { get; set; }
    public string ExpertiseArea { get; set; } = string.Empty;
    public int SensitivityLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AIIdentityListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
