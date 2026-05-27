# HelpDesk – Ticket Management System

## Project Description

HelpDesk is a web-based ticket management system created as a semester project for the Software Engineering course.

The application simulates a real internal company helpdesk environment where employees can report technical issues and communicate with IT support staff. The system supports ticket management, role-based authorization, notifications, comments, and ticket assignment workflows.

The project was developed using ASP.NET Core MVC (.NET 8) and Entity Framework Core with a SQL Server database.

---

# Main Features

## User Features

- User registration and login
- Creating support tickets
- Viewing own tickets
- Editing and deleting own tickets
- Adding comments to tickets
- Receiving notifications about updates

---

## Support Features

- Viewing all tickets
- Taking unassigned tickets
- Managing assigned tickets
- Changing ticket status
- Changing ticket priority
- Adding comments
- Receiving notifications

---

## Administrator Features

- Full access to the system
- Managing ticket categories
- Managing priorities
- Assigning tickets to support employees
- Managing all tickets
- Viewing all comments and notifications

---

# Technologies Used

## Backend
- C#
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- ASP.NET Identity

## Frontend
- Razor Views
- HTML5
- CSS3
- Bootstrap 5

## Database
- SQL Server

## Development Tools
- Visual Studio 2022
- GitHub
- Entity Framework Core Migrations

---

# System Roles

The application supports three user roles:

| Role | Description |
|---|---|
| User | Creates and manages own tickets |
| Support | Solves technical issues and manages assigned tickets |
| Admin | Full system access and ticket assignment |

---

# Database

The system uses a relational SQL Server database.

Main entities:
- Tickets
- Categories
- Priorities
- Notifications
- TicketComments
- Users

Entity Framework Core was used as the ORM layer.

---

# Running the Application

## Requirements

- Visual Studio 2022
- .NET 8 SDK
- SQL Server LocalDB

---

## Installation

### 1. Clone repository

```bash
git clone https://github.com/mrodziak/HelpDesk.git
```

---

### 2. Open solution in Visual Studio

Open the `.sln` file.

---

### 3. Update database

Open **Package Manager Console** and run:

```powershell
Update-Database
```

---

### 4. Run the application

Press:

```text
CTRL + F5
```

or click **Start** in Visual Studio.

---

## Administrator Accounts

- zuzanna.admin@firma.pl
- oliwier.admin@firma.pl

---

## Support Accounts

- szymon.it@firma.pl
- karolina.it@firma.pl
- martyna.it@firma.pl
- albert.it@firma.pl

---

## User Accounts

- kkowalska@firma.pl
- npawlowska@firma.pl
- aborkowska@firma.pl
- kratajczak@firma.pl
- hwachowiak@firma.pl
- lpoch@firma.pl
- sburak@firma.pl
- pkazmierczak@firma.pl

---

# Implemented Functionalities

- Authentication and authorization
- Role-based access control
- CRUD operations
- Ticket assignment system
- Comment system
- Notification system
- Status management
- Priority management
- Filtering tickets
- Responsive UI
- Database persistence

---

# UML Documentation

The project documentation contains:
- Use Case Diagram
- Activity Diagrams
- Class Diagram
- Sequence Diagrams
- Deployment / Architecture Diagram

---

Software Engineering – Semester 6  
Computer Science / Informatics  
2025/2026
