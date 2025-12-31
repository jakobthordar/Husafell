namespace Media.Presentation.DTOs;

/// <summary>
/// Request data for uploading a media asset.
/// </summary>
public sealed class UploadMediaRequest
{
    /// <summary>
    /// The file stream to upload.
    /// </summary>
    public Stream FileContent { get; set; } = null!;

    /// <summary>
    /// The name of the file being uploaded.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// The content type (MIME type) of the file.
    /// </summary>
    public string ContentType { get; set; } = null!;
}
