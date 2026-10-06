# cldv6212-cldv6212-poe-mutavhatsindimufunwa
# Group members:
- ST10470341 - Pavan Kumar Pundit
- ST10471202 - Cameron Jugdeo
- ST10487200 - Mufunwa Mutavhatsindi
  
# CoffeeNChill Cloud Development Project

## Project Overview

CoffeeNChill is a cloud based canteen management solution developed for CLDV6212 Cloud Development,The system modernises the canteen's menu management
and staff document storage processes by replacing manual paper-based processes with cloud-based services.
Azure Functions provide the application's HTTP API.
Azure Table Storage is used for menu data.
Azure File Share is used for staff document storage.
Docker and Azurite are used to provide a local containerised development and testing environment.
## Technologies Used
- C#
- .NET 10
- Azure Functions
- Azure Table Storage
- Azure File Share
- Docker
- Azurite
- Postman
- GitHub
- Visual Studio

## Application Architecture
Client
  |
  v
Azure Functions HTTP API
  |
  +----------------------+
  |                      |
  v                      v
TableStorageService   FileStorageService
  |                      |
  v                      v
Azure Table Storage   Azure File Share

The application separates HTTP Functions from storage operations through service classes and interfaces.

This separation improves maintainability and allows storage operations to be changed without placing storage logic directly inside the HTTP Functions.

System Features
Menu Management

The system provides:

Menu item creation
Menu item retrieval
Category filtering
Menu item updates
Menu item deletion
Input validation
Duplicate SKU detection
Not-found handling
Appropriate HTTP status codes
Staff Document Management
Staff document upload
Document listing
Document download
Document deletion
File metadata tracking
Not-found handling
My Contribution
Application Properties

# Pavan Pundit's contribution
Pavan Pundit was responsible for the application's configuration properties used during local development and Azure Storage integration. The configuration allows the application
to access the configured Azure Storage connection without hard-coding connection details into individual service methods.

Table Storage Service
I refactored and improved the Table Storage service responsible for menu management.

The service provides:
Menu item creation
Retrieval of all menu items
Category-based filtering
Individual menu item retrieval
Menu item updates
Menu item deletion

The implementation uses Azure Table Storage and handles missing entities appropriately.

The update and delete operations also use entity ETags to support Azure Table Storage concurrency behaviour.

File Storage Service
I refactored the staff document storage implementation to use Azure File Share.

The service provides:
File uploads
File downloads
File listing
File deletion
File metadata retrieval

The service validates that a usable file is supplied before attempting an upload.

Document metadata includes:
File name
File extension
MIME type
File size
Upload date
Storage share information
Storage Integration

I also verified the storage services against the containerised development environment.

The application was tested using:

Docker
+
Azure Functions
+
Azurite
+
Postman

The storage operations were verified through successful API requests and automated Postman tests.

Project Structure
CoffeeNChill
│
├── DTOs
├── Functions
├── Interfaces
├── Models
├── Properties
├── Services
├── docs
├── Dockerfile
└── README.md

## System Features

### Menu Management

The API supports:

- Create
- Read
- Update
- Delete
- Category filtering
- SKU-based retrieval
- Input validation
- Duplicate detection
- HTTP error handling

### Staff Document Management

The API supports:

- Document upload
- Document listing
- Document download
- Document deletion
- Document metadata

# Cameron Jugdeo's Contribution

## Azure Functions

I was responsible for developing and refining the HTTP-triggered Azure Functions.

My work included implementing the API endpoints used for menu management and staff document management.

The Functions handle:

- HTTP requests
- Request validation
- Service calls
- HTTP status codes
- Successful responses
- Error responses
- Not-found scenarios

The menu endpoints include:

POST   /api/menu
GET    /api/menu
GET    /api/menu/item/{category}/{sku}
GET    /api/menu/category/{category}
PUT    /api/menu/{category}/{sku}
DELETE /api/menu/{category}/{sku}

# Running the Application
## Prerequisites

### Install:

Visual Studio
.NET 10 SDK
Docker Desktop
Postman
Start Azurite
docker start azurite
Start CoffeeNChill Functions
docker start coffeechill-container
Verify Containers
docker ps

The Functions API is exposed on:

http://localhost:8080
Postman Testing

The Postman collection is available in the docs folder.

The collection contains:

Menu Management
Staff Documents

Automated tests cover successful operations and negative scenarios.

The final collection achieved:

53 Tests
0 Errors

The test suite covers menu CRUD operations, validation, duplicate detection, missing resources and the complete staff document lifecycle.

Docker

The project uses a multi-stage Dockerfile.

The build stage uses the .NET SDK to compile and publish the application.

The runtime stage uses the Azure Functions .NET isolated runtime.

The Functions container is exposed through port 8080 on the host.

Azurite provides local emulation of Azure Storage services.

Docker Hub

The project image was published using version tag:

v1.0


# Mufunwa Mutavhatsindi's contributions

### DTOs
I was responsible for developing and refining the Data Transfer Objects(DTOs) used by the application.

My work included:
- CreateMenuItemRequest
- UpdateMenuItemRequest
- MenuItemResponseDto
- StaffDocumentResponse
- Upload-related DTO structures

The DTOs provide structured data between HTTP Functions and the service layer.

### Interfaces
I was also responsible for defining and refining the service interfaces used by the application.

The interfaces were:
- ITableStorageService
- IFileStorageService

These interfaces establish the operations required by the storage services and help maintain separation of responsibilities within the application.

Furthermore, I provided the initial structure for the Project:
CoffeeNChill
│
├── DTOs
├── Functions
├── Interfaces
├── Models
├── Properties
├── Services
├── docs
├── Dockerfile
└── README.md

Finally I defined the .dockerignore file and Dockerfile which are very important for the use of docker and postman.


### Docker Hub repository:

[st10470341](https://hub.docker.com/u/st10470341)
Testing Evidence

Testing evidence is provided in the project documentation.

The evidence demonstrates:

Successful menu creation
Menu retrieval
Category filtering
Menu updates
Menu deletion
Validation failures
Duplicate SKU handling
Staff document upload
Staff document listing
Staff document download
Staff document deletion
Missing document handling
Docker container execution
Azure Functions running with Azurite
GitHub

# Repository:

(https://github.com/EMGPRS/cldv6212-cldv6212-poe-mutavhatsindimufunwa.git)
Video Demonstration

AI tools were used as a development support resource during the project, AI assistance was used to support technical understanding, review implementation approaches, identify possible improvements and assist with documentation.
The project group remained responsible for the final implementation and all code was reviewed, integrated and tested by the project group.

Conclusion

CoffeeNChill demonstrates how Azure Functions and Azure Storage can be used to modernise a small canteen management system.The solution separates API functionality from storage operations and provides a containerised local development environment using Docker and Azurite.The application was tested using an automated Postman collection covering both positive and negative API scenarios.
