namespace AiyoCoveX.Host.Models;

public class UploadedTempFile
{
    public string FileName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[]? BinaryContent { get; set; }
}
