namespace CleanArchitecture.Shared.Helpers;

public static class GuidHelper
{
    public static Guid NewGuidId(DateTimeOffset? dateTimeOffset = null)
    {
        var dto = dateTimeOffset ?? TimeProvider.System.GetUtcNow();
        return Guid.CreateVersion7(dto);
    }
}
