namespace Media.Presentation.DTOs;

/// <summary>
/// Data transfer object representing a media asset.
/// </summary>
/// <param name="Id">Unique identifier for the media asset.</param>
/// <param name="FileName">Name of the uploaded file.</param>
/// <param name="ContentType">MIME type of the media asset.</param>
/// <param name="UploadedAt">Timestamp when the asset was uploaded.</param>
/// <param name="Url">URL to access the media asset.</param>
public sealed record MediaAssetDto(
    Guid Id,
    string FileName,
    string ContentType,
    DateTime UploadedAt,
    string Url
);
