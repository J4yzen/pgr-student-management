# PGR Student Management: Week 2 Starter

## Overview

This project is the starting point for the Week 2 Postgraduate Research Student Management lab.

You will implement a small set of student-management use cases using:

- ASP.NET Core MVC
- A `Student` domain model
- An in-memory student collection
- Controllers and Razor views

The project intentionally contains incomplete actions and placeholder views. You are expected to complete these as part of the lab.

## Prerequisites

Before starting, ensure that you have:

- The required .NET SDK
- Visual Studio with the ASP.NET and web development workload, or Visual Studio Code with C# Dev Kit
- The Week 2 use-case document
- The supplied Student domain model

Check the installed .NET SDK:

```bash
dotnet --version
```

## Open the Project

Open the solution file:

```text
PgrStudentManagement.sln
```

Alternatively, open the complete `PgrStudentManagement` directory in Visual Studio Code.

## Build the Project

From the solution directory, run:

```bash
dotnet build
```

The build should complete without errors before you begin modifying the application.

## Run the Application

Run the MVC project:

```bash
dotnet run --project PgrStudentManagement.Web
```

Open the local address displayed in the terminal.

Select **Students** from the application navigation to view the supplied sample student records.

Stop the application by pressing:

```text
Ctrl+C
```

## Supplied Project Structure

```text
PgrStudentManagement/
├── PgrStudentManagement.sln
├── README.md
├── LabResources/
│   ├── week02-usecases-student-lab.md
│   ├── week02-domain-model-student.md
│   └── week02-lab1-instructions.md
└── PgrStudentManagement.Web/
    ├── Controllers/
    │   └── StudentsController.cs
    ├── Models/
    │   ├── Student.cs
    │   └── Status.cs
    ├── Views/
    │   ├── Students/
    │   └── Shared/
    ├── wwwroot/
    ├── Program.cs
    └── PgrStudentManagement.Web.csproj
```

## Implementation & Documentation

The Week 2 use cases (W2-UC01 to W2-UC05) have been implemented and tested.
A detailed description of the design decisions, field mapping, view implementations, alternative flows, and testing strategy can be found in:

- [README.md](README.md)

## Run Automated Tests

To run the automated xUnit test suite with standard summary:

```bash
dotnet test
```

To run with detailed step-by-step diagnostic output (`[GIVEN]`, `[WHEN]`, `[THEN]`):

```bash
dotnet test --logger "console;verbosity=detailed"
```
