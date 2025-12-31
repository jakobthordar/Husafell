# Shared.Web - Blazor Component Library

A shared Blazor component library for common UI elements, layouts, and styling used across all admin-facing features.

## Overview

This library provides reusable Blazor components following a consistent design system using Bootstrap 5.

## Installation

Add a reference to this project in your Blazor application:

```xml
<ProjectReference Include="..\Shared.Web\Shared.Web.csproj" />
```

## Setup

### 1. Register Services

In your `Program.cs`:

```csharp
using Shared.Web.Services;

builder.Services.AddSharedWebServices();
```

### 2. Add Imports

In your `_Imports.razor`:

```razor
@using Shared.Web.Components.Alerts
@using Shared.Web.Components.Common
@using Shared.Web.Components.Dialogs
@using Shared.Web.Components.Forms
@using Shared.Web.Components.Loading
@using Shared.Web.Components.Navigation
@using Shared.Web.Components.Tables
@using Shared.Web.Layouts
@using Shared.Web.Models
@using Shared.Web.Services
```

### 3. Include Styles

In your `index.html` or `_Host.cshtml`:

```html
<link href="_content/Shared.Web/Styles/admin.css" rel="stylesheet" />
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
```

## Components

### Layouts

#### AdminLayout
Main layout wrapper for admin pages with navigation and footer.

```razor
@layout AdminLayout

<PageHeader Title="Dashboard" Description="Welcome to the admin dashboard" />
```

### Navigation

#### AdminNavigation
Top/side navigation with links to modules.

#### ModuleMenu
Collapsible menu for module navigation.

```razor
<ModuleMenu 
    Title="Catalog" 
    Icon="bi-box-seam"
    Links="@catalogLinks" />
```

### Forms

#### TextInput
```razor
<TextInput 
    Label="Product Name"
    @bind-Value="productName"
    IsRequired="true"
    HelpText="Enter the product name"
    ErrorMessage="@validationError" />
```

#### FileUploadInput
```razor
<FileUploadInput 
    Label="Upload Image"
    Accept="image/*"
    OnFilesSelected="HandleFileUpload" />
```

#### SelectDropdown
```razor
<SelectDropdown 
    Label="Category"
    @bind-Value="selectedCategory"
    Options="@categoryOptions" />
```

### Dialogs

#### ConfirmDialog
```razor
<ConfirmDialog 
    IsVisible="@showConfirm"
    Title="Delete Product"
    Message="Are you sure you want to delete this product?"
    OnConfirm="HandleDelete"
    OnCancel="() => showConfirm = false" />
```

#### AlertDialog
```razor
<AlertDialog 
    IsVisible="@showAlert"
    Title="Success"
    Message="Product saved successfully"
    Type="AlertType.Success"
    OnClose="() => showAlert = false" />
```

### Loading

#### LoadingSpinner
```razor
<LoadingSpinner Message="Loading data..." />
```

#### SkeletonLoader
```razor
<SkeletonLoader Width="100" Height="20" />
```

### Tables

#### DataTable
```razor
<DataTable TItem="Product" Items="@products" ShowPagination="true">
    <HeaderTemplate>
        <th>Name</th>
        <th>Price</th>
        <th>Actions</th>
    </HeaderTemplate>
    <RowTemplate Context="product">
        <td>@product.Name</td>
        <td>@product.Price</td>
        <td>
            <button @onclick="() => Edit(product)">Edit</button>
        </td>
    </RowTemplate>
</DataTable>
```

### Alerts

#### Alert
```razor
<Alert 
    Message="Operation successful!"
    Type="AlertType.Success"
    IsDismissible="true" />
```

#### Toast / ToastContainer
```razor
@* In your layout or main component *@
<ToastContainer />

@* Usage with NotificationService *@
@inject NotificationService NotificationService

@code {
    private void SaveData()
    {
        // Your logic
        NotificationService.ShowSuccess("Data saved successfully!");
    }
}
```

### Common

#### PageHeader
```razor
<PageHeader Title="Products" Description="Manage your product catalog">
    <Actions>
        <button class="btn btn-primary">Add Product</button>
    </Actions>
</PageHeader>
```

#### EmptyState
```razor
<EmptyState 
    Icon="bi-inbox"
    Title="No Products"
    Message="You haven't added any products yet.">
    <Actions>
        <button class="btn btn-primary">Add Your First Product</button>
    </Actions>
</EmptyState>
```

## Services

### NotificationService

```csharp
@inject NotificationService NotificationService

NotificationService.ShowSuccess("Operation completed!");
NotificationService.ShowError("An error occurred!");
NotificationService.ShowInfo("Here's some information.");
NotificationService.ShowWarning("Please be careful!");
```

## Styling

The library uses CSS variables for theming. You can override these in your application:

```css
:root {
    --primary-color: #your-color;
    --sidebar-width: 280px;
    /* etc. */
}
```

## Icons

This library uses Bootstrap Icons. Make sure to include the Bootstrap Icons stylesheet in your application.

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
```
