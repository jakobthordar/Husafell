namespace Media.Presentation.Services;

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Media.Presentation.DTOs;

/// <summary>
/// HTTP client wrapper for the Media API.
/// </summary>
public class MediaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MediaApiService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for Media.Api.</param>
    /// <param name="logger">The logger instance.</param>
    public MediaApiService(HttpClient httpClient, ILogger<MediaApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets all media assets.
    /// </summary>
    /// <returns>A collection of media assets.</returns>
    public async Task<IReadOnlyCollection<MediaAssetDto>> GetAssetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all media assets");
            var assets = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<MediaAssetDto>>(
                "/api/media/assets",
                cancellationToken: cancellationToken);
            return assets ?? Array.Empty<MediaAssetDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch media assets");
            throw;
        }
    }

    /// <summary>
    /// Gets a media asset by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the media asset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The media asset, or null if not found.</returns>
    public async Task<MediaAssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching media asset with ID: {AssetId}", id);
            return await _httpClient.GetFromJsonAsync<MediaAssetDto>(
                $"/api/media/assets/{id}",
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch media asset with ID: {AssetId}", id);
            throw;
        }
    }

    /// <summary>
    /// Uploads a new media asset.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded media asset.</returns>
    public async Task<MediaAssetDto> UploadAssetAsync(
        Stream file,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Uploading media asset: {FileName}", fileName);

            var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("/api/media/upload", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var asset = await response.Content.ReadFromJsonAsync<MediaAssetDto>(cancellationToken: cancellationToken);
            return asset ?? throw new InvalidOperationException("Failed to deserialize upload response");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to upload media asset: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a media asset.
    /// </summary>
    /// <param name="id">The unique identifier of the media asset to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting media asset with ID: {AssetId}", id);
            var response = await _httpClient.DeleteAsync($"/api/media/assets/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to delete media asset with ID: {AssetId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets the URL for a media asset.
    /// </summary>
    /// <param name="id">The unique identifier of the media asset.</param>
    /// <returns>The URL to access the media asset.</returns>
    public string GetAssetUrl(Guid id)
    {
        return $"/api/media/assets/{id}/content";
    }

    /// <summary>
    /// Gets the download URL for a media asset.
    /// </summary>
    /// <param name="id">The unique identifier of the media asset.</param>
    /// <returns>The URL to download the media asset.</returns>
    public string GetDownloadUrl(Guid id)
    {
        return $"/api/media/assets/{id}/download";
    }
}
