namespace JBlam.Multiflash
{
    // TODO: some parameters are logically optional (e.g. deliberately blank blank wifi == insecure access point)
    public record Parameter(string Identifier, string Label, string? Description = null);
}
