# NipponQuest

NipponQuest is a gamified full stack Japanese language learning platform developed by YugenLabs using **ASP.NET Core MVC**. The platform transforms traditional Hiragana and Katakana study into an immersive quest-based learning experience through progression systems, achievement tracking, and interactive educational modules.

Designed with both engagement and accessibility in mind, NipponQuest combines modern web technologies with educational gamification principles to create a more motivating approach to language acquisition.

---

# Project Overview

NipponQuest was developed as part of a Bachelor of Computer Applications (BCA) academic project, showcasing the practical implementation of enterprise-level software development concepts including:

- MVC architectural design patterns
- Object-oriented programming with C#
- Secure authentication and authorization
- Entity Framework Core data persistence
- Responsive web application development
- Gamification systems and user engagement logic

The application emphasizes scalable architecture, maintainable code structure, and modern full stack development practices.

---

# Core Features

## Gamified Progression System
- Custom XP and leveling mechanics
- Rank progression based on learner activity
- Achievement-driven learning experience
- Persistent progression tracking

## Japanese Kana Learning Modules
- Interactive Hiragana practice
- Interactive Katakana practice
- Real-time feedback and validation
- Structured character mastery system

## User Authentication & Profiles
- Secure authentication using ASP.NET Core Identity
- Personalized learner dashboards
- Persistent user data and progression storage
- Role-based authentication support

## Leaderboards & Competition
- Global ranking system
- XP-based leaderboard tracking
- Study streak comparisons
- Community engagement features

## Daily Streak Tracking
- Consistency-based reward mechanics
- Automated streak calculation logic
- Retention-focused engagement systems

## Responsive User Interface
- Mobile-first responsive design
- Built with Bootstrap and custom CSS
- Optimized for desktop, tablet, and mobile devices
- Clean and accessible UI/UX design

---

# Technical Stack

| Technology | Purpose |
|---|---|
| .NET 10 | Application Framework |
| ASP.NET Core MVC | Backend Architecture |
| Entity Framework Core | ORM & Database Management |
| SQL Server | Relational Database |
| ASP.NET Core Identity | Authentication & Security |
| Razor Views | Server-side Rendering |
| HTML5 / CSS3 / JavaScript | Frontend Development |
| Bootstrap | Responsive UI Framework |
| Git & GitHub | Version Control |

---

# Project Architecture

```text
NipponQuest-main/
├── Areas/Identity/      # Authentication and Identity management
├── Controllers/         # Request handling and routing logic
├── Data/                # DbContext, migrations, and seed data
├── Models/              # Domain models and entities
├── Services/            # Business logic and application services
├── Views/               # Razor view templates
├── wwwroot/             # Static assets and frontend resources
└── NipponQuest.csproj   # Project configuration
```

---

# Installation & Setup

## Prerequisites

Before running the project locally, ensure the following tools are installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
- Visual Studio 2022 or Visual Studio Code

---

## Local Installation

### 1. Clone the Repository

```bash
git clone https://github.com/marclourens19/NipponQuest.git
```

### 2. Navigate to the Project Directory

```bash
cd NipponQuest/NipponQuest-main
```

### 3. Configure the Database Connection

Update the `DefaultConnection` string inside `appsettings.json` to match your local SQL Server configuration.

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The application will be available at:

```text
https://localhost:7000
```

---

# Development Objectives

This project demonstrates proficiency in:

- Full stack ASP.NET Core MVC development
- Relational database design and management
- Entity Framework Core code-first migrations
- Dependency Injection and service architecture
- Middleware integration and request pipeline customization
- Authentication and authorization implementation
- Responsive frontend development
- Scalable application structure and maintainable code practices

---

# Future Development

Planned improvements and future expansion areas include:

- Vocabulary and grammar learning modules
- AI-assisted learning recommendations
- Multiplayer learning challenges
- Achievement and badge systems
- Audio pronunciation integration
- Advanced analytics and learner insights
- Japanese sentence construction exercises

---

# Author

Developed by **Joshua Marc Lourens**  
Bachelor of Computer Applications (BCA) Student  
Founder & Developer at **YugenLabs**

---

# License

This project is intended for educational and portfolio purposes. Additional licensing terms may be added in future releases.
