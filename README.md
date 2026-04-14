# Lab 6: Library Management System

A Blazor web application for managing books, users, and borrowing activity, with an accompanying xUnit test project.

## Overview

This project converts a basic library management workflow into a web application using ASP.NET Core Blazor. The application allows users to manage a small in-memory and CSV-backed library system through a clean browser interface.

## Features

- Add, edit, list, and delete books
- Add, edit, list, and delete users
- Borrow books by assigning them to a user
- Return borrowed books
- Display currently borrowed books by user
- Persist book and user data with CSV files
- Validate service behavior with automated unit tests

## Project Structure

```text
Lab 6/
|-- Lab 5/                  Main Blazor application
|   |-- Components/         Layout and page components
|   |-- Data/               CSV seed data
|   |-- Models/             Domain models
|   |-- Services/           Library service logic
|   `-- wwwroot/            Static assets
|-- Lab 5.Tests/            xUnit test project
|-- Lab 5.slnx              Solution file
`-- README.md               Repository documentation
```

## Technologies Used

- ASP.NET Core Blazor
- C#
- xUnit
- .NET 8

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)

### Run the Application

```bash
dotnet run --project "Lab 5/Lab 5.csproj"
```

Then open the local URL shown in the terminal.

### Run the Tests

```bash
dotnet test "Lab 5.Tests/Lab 5.Tests.csproj"
```

## Pages

- `Home`: summary and navigation hub
- `Books`: manage the library book catalog
- `Users`: manage registered users
- `Borrow / Return`: track book loans and returns

## Testing Summary

The test project covers the main service behaviors, including:

- loading books and users from CSV files
- adding, editing, and deleting records
- borrowing available books
- preventing duplicate borrowing
- returning borrowed books
- cleaning up borrowed records when books or users are removed

## Notes

- The repository name is `Lab 6`, while the project folders still use `Lab 5` because the assignment builds on the previous lab.
- Visual Studio cache files are excluded with `.gitignore` so the repository stays clean and commit-friendly.

## Author

Daniel
