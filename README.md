TourPlanner
=====================================

The TourPlanner application is a route planning tool that allows users to create, manage, and visualize tours with detailed logs. It features a WPF-based UI, a RESTful API backend, and integrates with external services for route calculations and PDF report generation. The application is designed using a layered architecture, promoting separation of concerns and maintainability.

# 1. How to set up and use the TourPlanner

  - Start Docker
  - Navigate to the **Database/** directory and adjust the **docker-compose.yml** file if necessary (e.g., change the password for the PostgreSQL user).
  - Execute **docker-compose up -d** to start the containerized PostgreSQL database
  - Copy appsettings.example.json as appsettings.json in the **TourPlanner.API** directory and adjust the connection string to match your PostgreSQL database configuration and enter your OpenRouteService API key.
  - Build the Solution.
  - Start the server by running **TourPlanner.API.exe**.
  - Launch the application by running **TourPlanner.UI.exe** 
  - Have fun and start planning your routes!


# 2. Design and Architecture

The TourPlanner project is built using a **Layered Architecture**, which promotes separation of concerns and maintainability. It is divided into six main components:

- **UI Layer (`TourPlanner.UI`)**  
  A WPF-based frontend that follows the MVVM pattern, handling user interactions, data binding, and visualization (including Leaflet-based map rendering).

- **API Layer (`TourPlanner.API`)**  
  An ASP.NET Core web service exposing RESTful endpoints to the frontend and forwarding client requests to the business logic.

- **Business Logic Layer (`TourPlanner.BLL`)**  
  Encapsulates core application logic, validation rules, and external API integration (e.g., OpenRouteService for routing and GeoJSON generation).

- **Data Access Layer (`TourPlanner.DAL`)**  
  Manages database access via Entity Framework Core, implementing repository patterns and mapping entities to business models.

- **Logging Layer (`TourPlanner.Logging`)**  
  Provides a centralized, abstracted logging mechanism used across all application layers with support for dependency injection.

- **Test Solution (`TourPlanner.Test`)**  
  Contains unit tests for ViewModels and services using NUnit and NSubstitute to ensure correctness and maintainability across all layers.

This architecture ensures a clean structure, with each layer having a clear responsibility and depending only on the layer directly below it.


## 2.1 TourPlanner.UI

The TourPlanner.UI project represents the WPF-based UI layer of the application. It is responsible for displaying the tour data, handling user interaction, and coordinating with the underlying services. It follows the MVVM pattern to separate concerns between views and logic, and it uses data binding, commands, and event-based navigation to keep the interface responsive and maintainable.

### 2.1.1 Constraints

- **Add Views:**
  - A Tour Log can only be added if a Tour is selected.
  - AddNewTourView can’t be displayed when a tour is being edited.
  - AddNewTourLogView can’t be displayed when a tour log is being edited.
  - Forms of AddNewTourView and AddNewTourLogView must have all fields filled to enable the save button and execute the save commands.
  - AddNewTourLogView closes automatically after the selected tour changes.

- **Edit Views:**
  - EditTour button is only available when a Tour is selected, and the tour doesn’t have any tour logs attached.
  - EditTourLog button is only available when a TourLog is selected.
  - EditTourView can’t be displayed when a new tour is being added.
  - EditTourLogView can’t be displayed when a new tour log is being added.
  - Save button and save command are only available if all fields are filled out and something was actually changed.
  - EditTourView and EditTourLogView close automatically after the selected tour changes.

### 2.1.2 Input Validation
User input is validated directly in the ViewModels using CanExecute logic on RelayCommand bindings. Fields must be filled out and valid (e.g., non-empty strings, positive numbers) to enable the Save buttons. The RaiseCanExecuteChanged() method ensures that UI buttons are only enabled when input is valid.

### 2.1.3 App.xaml.cs
This file is responsible for the application's startup logic and sets up the **MainWindow** along with its associated **ViewModels**. It initializes the core services:
- HttpService
- TourDataService
- SelectedTourService
- UiCoordinator
- FileDialogService
- MapService
- TourReportPdfService
- TourSummaryPdfService
- Dialogservice
- ApplicationService

### 2.1.4 Commands
This folder contains command implementations, such as RelayCommand, which provides a reusable way to bind UI actions (like button clicks) to logic in ViewModels.

### 2.1.5 Converters
Contains WPF converters used in data bindings to transform values. Examples include BooleanToVisibilityConverter and BooleanToGridLengthConverter, which help display or hide dynamically created UI elements (Add- and Edit-Views) based on boolean values.

### 2.1.6 Models
Defines the core data structures used in the UI, such as Tour and TourLog. These models represent individual tours and their logs.

### 2.1.7 Resources
Currently holds static resources like the application icon. This directory is used for UI-related assets that are needed for rendering views.

### 2.1.8 Services
The **Services** directory contains classes that handle data management, UI coordination, and other core functionalities. These services act as intermediaries between the ViewModels and the underlying business logic or external APIs.

- **TourDataService**  
  Manages the retrieval, addition, updating, and deletion of tours and tour logs. It communicates with the backend API using the **HttpService** and provides search and filtering functionality for tours. It also supports importing and exporting tours in JSON format.

- **SelectedTourService**  
  Tracks the currently selected tour and tour log. It raises events when the selection changes, allowing other components to react accordingly. This service is essential for maintaining the application's state across different views.

- **UiCoordinator**  
  Handles navigation and coordination between different views, such as opening and closing the Add/Edit Tour and Tour Log views. It raises events to notify the UI when a specific view needs to be displayed or hidden.

- **MapService**  
  Manages map-related functionality, including loading GeoJSON data into a Leaflet map, clearing the map, and capturing map screenshots for use in PDF reports. It uses the **WebView2** control to render maps dynamically.

- **TourReportPdfService**  
  Generates detailed PDF reports for individual tours. These reports include tour details, an embedded map screenshot, a list of tour logs, and statistical information such as average time, distance, and rating. It uses the **iText7** library for PDF creation.

- **TourSummaryPdfService**  
  Creates an aggregate PDF report summarizing all available tours. The report includes statistical analysis for each tour, such as average time, distance, rating, and difficulty, along with child-friendliness and popularity indicators.

- **HttpService**  
  Provides a reusable HTTP client for making API requests to the backend. It supports GET, POST, PUT, and DELETE operations, as well as handling raw JSON data for import/export functionality.

- **FileDialogService**  
  Handles file dialog interactions, such as opening and saving files. It is used for importing/exporting tours and specifying save locations for PDF reports.

- **DialogService**  
  Displays informational, error, and confirmation dialogs to the user. It provides a simple interface for showing messages and capturing user responses.

- **ApplicationService**  
  Provides application-level functionality, such as shutting down the application gracefully.


### 2.1.9 Views

The **Views** directory contains the XAML files that define the user interface for the application. Each view is responsible for a specific part of the UI and is designed to work with its corresponding ViewModel in the MVVM pattern.

- **MainWindow.xaml**  
  The main window of the application, defining the overall layout. It primarily acts as a container for other views, such as the `MenuBarView`, `SearchView`, `TourListView`, `TourInfoView`, and `TourLogsView`.

- **MenuBarView.xaml**  
  Defines the menu bar at the top of the application. It provides options for importing/exporting tours, generating reports, and performing actions like adding, editing, or deleting tours and tour logs. It also includes a "Help" section and an "Exit" option.

- **SearchView.xaml**  
  Provides a search bar for filtering tours. It includes a text box for entering search queries and buttons for executing the search or resetting the filter.

- **TourListView.xaml**  
  Displays a list of available tours, allowing users to select, add, remove, or edit a tour. It uses a `ListBox` for displaying tour names and provides buttons for managing tours.

- **TourInfoView.xaml**  
  Shows detailed information about the selected tour, including name, description, distance, estimated time, and popularity. It also includes a map view (Leaflet) to display the route visually and an additional tab displaying the directions.

- **TourLogsView.xaml**  
  Displays logs related to the selected tour, showing date, comments, distance, time, difficulty, and rating. Users can add, remove, or edit logs through this view.

- **EditTourView.xaml**  
  Provides a form to modify an existing tour. It allows users to change the name, description, route, transport type, and other details before saving.

- **EditTourLogView.xaml**  
  A form for editing an existing tour log, including modifying comments, difficulty, rating, and travel time.

- **AddNewTourView.xaml**  
  A form to create a new tour, allowing users to input basic tour details such as name, description, route, and transport type.

- **AddNewTourLogView.xaml**  
  A form for adding a new tour log, enabling users to document experiences by specifying distance, time, difficulty, and personal comments.

- **Leaflet.html**  
  Visualizes tour routes using **Leaflet** and dynamic GeoJSON data inside a **Microsoft WebView2** control. The Leaflet.html template is loaded and filled with the selected tour's route to display an interactive **OpenStreetMap**, with the WebView2 component enabling modern web rendering and screenshot capture for PDF reports.

### 2.1.10 ViewModels

The ViewModels act as the data-binding layer between the Views and the business Services. ViewModels manage the application's state and user interactions by exposing commands and properties.

- **MainWindowViewModel.cs**  
  The main entry point of the application. It orchestrates navigation between views and manages the state of different ViewModels (**TourListViewModel**, **TourLogsViewModel**, **EditTourViewModel**, etc.). It listens to UI coordination events to open or close specific views dynamically.

- **MenuBarViewModel.cs**  
  Handles the logic for the menu bar, including commands for importing/exporting tours, generating reports (Tour Report and Tour Summary), and exiting the application.

- **SearchViewModel.cs**  
  Manages the search functionality for tours. It provides a command to filter the tour list based on user input and updates the displayed tours accordingly.

- **TourInfoViewModel.cs**
  Displays details of a selected tour, such as direction steps, name, description, distance, popularity, child-friendliness and most importantly the map.

- **TourListViewModel.cs**
  Manages the list of tours, allowing users to add, remove, or edit tours. It provides commands for tour actions, updates selections, and binds to the ListBox in the UI.

- **TourLogsViewModel.cs**
  Manages logs for a selected tour. It allows adding, editing, and removing tour logs, with real-time updates to the UI.

- **AddNewTourViewModel.cs**
  Handles the creation of a new tour. It provides a SaveCommand that adds a new tour to the ITourDataService and closes the view after completion.

- **EditTourViewModel.cs**
  Allows modifying an existing tour, tracking changes, and updating the tour details in ITourDataService.

- **AddNewTourLogViewModel.cs**
  Manages the creation of new tour logs by binding user inputs (distance, difficulty, rating, etc.) and saving the log under the selected tour.

- **EditTourLogViewModel.cs**
  Handles the modification of an existing tour log, ensuring only valid changes are saved while keeping track of the original values.


## 2.2 TourPlanner.API

The **API Layer** provides a RESTful interface for the TourPlanner application, exposing endpoints for managing tours and tour logs. It is implemented using **ASP.NET Core** and acts as the entry point for all HTTP-based interactions with the backend.

- Uses **Tour and TourLog Controllers** to map HTTP routes to application logic.
- Returns and accepts **JSON-serialized domain objects**.
- Configured to support **Swagger/OpenAPI** for API documentation.
- Depends only on the **Business Logic Layer (BLL)** to process requests.
- Receives data from the UI and passes it to the BLL for validation and processing.

The complete API endpoints documentation is available as an OpenAPI (Swagger) specification in the Documentation directory (`TourPlanner.API_Swagger.json`).

## 2.3	TourPlanner.BLL

The **Business Logic Layer (BLL)** encapsulates the core logic of the application. It defines interfaces and services that manage how data flows between the API, the external APIs and the database.

- Defines **Domain Models** (e.g., `TourDomain`, `TourLogDomain`) which represent the application’s core business entities.
- Implements **Service Interfaces** like `ITourService` and `ITourLogService` to provide a clean and testable abstraction.
- Contains business rules (e.g., which tours can be edited, validation rules, etc.).
- Delegates data access to the **Data Access Layer (DAL)**.
- Maps the **DAL** entities to domain models using a custom IMapper implementation in the **BLL**.
- Integrates **RouteService** to calculate and format route information (distance, time, and directions) using the **OpenRouteService API** during tour creation and editing.
- Additionally, the **RouteService** saves the GeoJson received from the **OpenRouteService API** into the Tour (used for displaying the route via Leaflet in the UI).


## 2.4	TourPlanner.DAL

The **Data Access Layer (DAL)** handles all communication with the PostgreSQL database using **Entity Framework Core** as an Object-Relational Mapper (ORM).

### Key Components:

- **Entity Classes**: Defines domain entities like `TourEntity` and `TourLogEntity` that map to database tables with proper data annotations
  
- **DbContext Configuration**: Uses a robust `TourPlannerDbContext` with Fluent API configuration in `OnModelCreating` to:
  - Define schema (`tourplanner`)
  - Configure table names, property constraints, and relationships
  - Set up indexes for performance optimization
  - Configure cascade delete behavior for related entities

- **Entity Framework Migrations**: Implements code-first database management, allowing schema evolution to match model changes:
  - Auto-creates database schema during application startup
  - Tracks applied migrations through the `__EFMigrationsHistory` table
  - Supports version-controlled schema updates

- **Repository Pattern**: Implements repository interfaces (`ITourRepository`, `ITourLogRepository`) to abstract database access:
  - Provides CRUD operations for tours and tour logs
  - Handles database exceptions and wraps them in domain-specific exceptions
  - Supports async database operations throughout

- **Docker Integration**: Works with PostgreSQL in Docker, with migrations automatically applied at application startup


## 2.5 TourPlanner.Logging

The **TourPlanner.Logging** library provides a centralized, abstracted logging mechanism used across the backend layers of the application (**API**, **BLL**, and **DAL**). It implements best practices for logging by decoupling the logging concerns from specific implementations.

### Key Features:

- **Abstraction Layer**: Defines the `ILoggerWrapper<T>` interface that abstracts away the underlying logging framework (Log4Net), allowing for easy replacement without modifying application code.

- **Contextual Logging**: Provides class context through generic typing (`ILoggerWrapper<T>`), making it easy to identify the source of log messages.

- **Custom Log Formatting**: Uses custom colors for different log levels (e.g., Debug, Info, Warning, Error, Fatal) to improve readability in the console.

- **File Logging**: Saves all log messages to a file for persistent storage and debugging purposes.

- **Integration**: Integrated with the backend layers via dependency injection, ensuring consistent logging behavior across the application.

- **Separation of Concerns**: Completely isolates logging configuration (**log4net.config**) from application code.


## 2.6 TourPlanner.Tests

The test solution implements unit tests for all layers of the **TourPlanner** project. The tests are designed using **NUnit** and **NSubstitute**, following best practices by mocking dependencies to isolate unit testing.

### TourPlanner.API Tests

Tests focus on controller behavior, ensuring proper HTTP status codes and response objects:
- Verify correct handling of successful operations (returning appropriate status codes like 200, 201)
- Validate error handling for various exception types (404 for not found, 400 for bad requests)
- Test parameter validation and proper service method invocation
- Ensure controllers properly delegate to their corresponding services with correct parameters

### TourPlanner.BLL Tests

Tests verify the core business logic implementation:
- Validate service methods correctly interact with repositories and external services
- Ensure proper exception wrapping and propagation occurs (repository exceptions → service exceptions)
- Test the mapping logic between domain models and entity objects
- Verify the RouteService correctly interacts with the OpenRouteService API and handles response data
- Test validation rules and business constraints during tour and tour log operations

### TourPlanner.DAL Tests

Tests verify data access operations:
- Ensure repository methods perform expected operations on the database context
- Test entity relationships and data integrity constraints
- Validate exception handling for database operations
- Test CRUD operations for tour and tour log entities

### TourPlanner.UI Tests

Tests focus on ViewModel behavior:
- Validate property initialization and binding behavior
- Test command execution and validation logic (CanExecute conditions)
- Ensure services are properly called when command execution occurs
- Test constraint enforcement (e.g., SaveCommand disabled when inputs are invalid)
- Verify correct UI coordination between views during navigation


## 3 Tour Report and Tour Summary Generation

The TourPlanner application provides PDF report generation capabilities through two specialized services. The **Tour Report** feature (`TourReportPdfService`) creates detailed documents for individual tours, including tour details, an embedded map screenshot captured from the **Leaflet WebView2 control**, a complete listing of tour logs, and statistical information (average time, distance, and rating). The **Tour Summary** feature (`TourSummaryPdfService`) generates an aggregate report of all available tours, presenting statistical analysis for each tour derived from associated logs. Both reports use the **iText7** library for professional PDF creation with structured tables, formatted text, and consistent styling. Reports are generated through menu commands in the `MainWindowViewModel`, with file dialogs allowing users to specify save locations.


# 4. Github Actions

The TourPlanner project implements a comprehensive CI/CD pipeline using GitHub Actions to automate build, test, and release processes. The workflow is defined in pipeline.yaml and consists of three main jobs:

## 4.1 Build Job
- Triggers on pushes to main and develop branches, pull requests, and manual workflow dispatch
- Runs on Windows latest environment
- Sets up .NET 8.0 SDK
- Restores dependencies and builds the solution in Release configuration
- Archives build artifacts for subsequent jobs

## 4.2 Test Job
- Depends on successful completion of the build job
- Downloads the build artifacts
- Executes unit tests using the NUnit test framework
- Ensures code quality and functionality before proceeding to release

## 4.3 Create-Release Job
- Only runs after successful testing and when changes are pushed to the main branch
- Packages the application binaries along with:
  - Database configuration files for PostgreSQL setup
  - README.md documentation for installation and usage instructions
- Creates a versioned ZIP release artifact containing all necessary files to deploy the application
- Publishes the release to GitHub with timestamp information
- Replaces any existing "latest" release tag to maintain a clean release history