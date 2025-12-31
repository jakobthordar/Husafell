# Media.Presentation

A Blazor component library for the Media module that encapsulates all UI components and services for media asset management.

## Overview

This library provides reusable Blazor components for managing media assets, including:

- **MediaAssetsList** - Display all media assets with filtering and search
- **MediaAssetUpload** - File upload component with progress indication
- **MediaAssetDetail** - Show single asset details and preview
- **MediaAssetForm** - Form for managing asset metadata
- **MediaPreview** - Display images or generic file icons based on content type

## Project Structure

```
Media.Presentation/
├── Components/
│   ├── Assets/
│   │   ├── MediaAssetsList.razor
│   │   ├── MediaAssetUpload.razor
│   │   ├── MediaAssetDetail.razor
│   │   └── MediaAssetForm.razor
│   └── Shared/
│       └── MediaPreview.razor
├── Services/
│   ├── MediaApiService.cs
│   └── MediaPresentationServiceExtensions.cs
├── DTOs/
│   ├── MediaAssetDto.cs
│   └── UploadMediaRequest.cs
└── Media.Presentation.csproj
```

## Installation

Add the Media.Presentation project to your solution:

```csharp
// In your ServiceCollection configuration
builder.Services.AddMediaPresentation();
```

## Components

### MediaAssetsList

Displays a grid of media assets with search functionality.

**Parameters:**
- None (uses `MediaApiService` to load data)

**Features:**
- Search/filter assets by name
- Click on asset card to view details
- Upload button for new assets
- Empty state when no assets exist

### MediaAssetUpload

File upload component with drag-and-drop support.

**Parameters:**
- None (uses `MediaApiService` to upload files)

**Features:**
- Drag and drop file upload
- File preview for images
- Upload progress indication
- Error handling and validation

### MediaAssetDetail

Shows detailed information about a single media asset.

**Parameters:**
- `Guid AssetId` - The ID of the asset to display

**Features:**
- Asset preview (image, video, audio, or file icon)
- Download and delete actions
- Edit metadata
- Copy URL and embed code

### MediaAssetForm

Form for managing asset metadata.

**Parameters:**
- `MediaAssetDto? Asset` - The asset being edited (optional for new uploads)
- `bool IsNew` - Indicates if this is a new asset upload
- `EventCallback<MediaAssetDto> OnSave` - Callback when form is submitted
- `EventCallback OnCancel` - Callback when form is cancelled

**Features:**
- File name editing
- Content type selection (read-only after upload)
- Alt text, tags, and description fields
- Validation

### MediaPreview

Displays a preview of media assets based on content type.

**Parameters:**
- `string Url` - The URL of the media asset
- `string ContentType` - The MIME type of the media
- `string FileName` - The name of the file

**Features:**
- Image preview for image/* MIME types
- Video player for video/* MIME types
- Audio player for audio/* MIME types
- PDF viewer for application/pdf
- File type icons for other types

## Services

### MediaApiService

HTTP client wrapper for the Media API.

**Methods:**
- `GetAssetsAsync()` - Get all media assets
- `GetAssetByIdAsync(Guid id)` - Get a specific asset
- `UploadAssetAsync(Stream file, string fileName, string contentType)` - Upload a new asset
- `DeleteAssetAsync(Guid id)` - Delete an asset
- `GetAssetUrl(Guid id)` - Get the content URL for an asset
- `GetDownloadUrl(Guid id)` - Get the download URL for an asset

## Data Transfer Objects

### MediaAssetDto

```csharp
public sealed record MediaAssetDto(
    Guid Id,
    string FileName,
    string ContentType,
    DateTime UploadedAt,
    string Url
);
```

### UploadMediaRequest

```csharp
public sealed class UploadMediaRequest
{
    public Stream FileContent { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}
```

## Usage Example

```razor
@page "/media"
@using Media.Presentation.Components.Assets
@inject Media.Presentation.Services.MediaApiService MediaApiService

<MediaAssetsList />
```

## Configuration

The `AddMediaPresentation()` extension method configures the HttpClient with the base address `http://media-api`.

To customize the base address:

```csharp
builder.Services.AddHttpClient<Media.Presentation.Services.MediaApiService>(client =>
{
    client.BaseAddress = new Uri("https://your-media-api-url");
});
```

## Dependencies

- .NET 9.0
- Microsoft.AspNetCore.Components.Web 9.0.0
- Microsoft.Extensions.Http 9.0.0
- Media.Application

## Styling

Components use Bootstrap 5 classes for styling. Ensure Bootstrap 5 CSS and JavaScript are included in your hosting application.
