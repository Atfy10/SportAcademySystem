namespace SportAcademy.Domain.Enums;

// Closes off the storage subfolder to a fixed, known set rather than accepting an arbitrary
// caller-supplied string - IFileStorageService maps each value to one hardcoded folder name, so
// there is no path-construction input to sanitize or validate in the first place.
public enum ImageUploadCategory
{
    /// <summary>A user's own account avatar (Profile.ProfileImageUrl).</summary>
    Avatar,
    /// <summary>An Employee/Trainee/Coach record's photo (Person.ImageUrl).</summary>
    Person,
    /// <summary>A tenant's organization logo (TenantProfile.LogoUrl).</summary>
    TenantLogo,
}
