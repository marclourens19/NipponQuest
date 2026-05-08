# NipponQuest 

NipponQuest is a gamified full-stack Japanese language learning platform built using **ASP.NET Core MVC**. It transforms the traditional study of Hiragana and Katakana into an interactive "quest" experience, utilizing a persistent leveling system to drive student engagement and retention.

## 🚀 Project Vision
As a student project for the Bachelor of Computer Applications (BCA) program, NipponQuest demonstrates the practical application of the **Model-View-Controller (MVC)** architectural pattern, object-oriented programming in C#, and secure data persistence using Entity Framework Core.

## ✨ Key Features

* **RPG-Style Gamification:** A custom-built Experience Point (XP) and Leveling system. Users advance through ranks as they master new characters.
* **Persistent User Progress:** Integrated **ASP.NET Core Identity** for secure authentication and personalized dashboard tracking.
* **Kana Mastery Modules:** Interactive learning sections for both Hiragana and Katakana with real-time feedback.
* **Leaderboard System:** A competitive module that ranks students based on their study streaks and total XP earned.
* **Streak Tracking:** Logic-based daily streaks to encourage consistent learning habits.
* **Responsive UI:** A mobile-first design implemented with **Bootstrap** and custom **CSS3**, ensuring the platform is accessible on any device.

## 🛠️ Technical Stack

* **Framework:** .NET 10.0 (LTS)
* **Architecture:** ASP.NET Core MVC
* **Database:** SQL Server / Entity Framework Core (Code-First Approach)
* **Security:** ASP.NET Core Identity (Password hashing, Cookie authentication)
* **Frontend:** Razor Pages, HTML5, CSS3, JavaScript/jQuery
* **Development Tools:** Visual Studio 2022, Git

## 📂 Project Structure

```text
NipponQuest-main/
├── Areas/Identity/      # ASP.NET Core Identity pages and logic
├── Controllers/         # Application flow and routing logic
├── Data/                # DbContext, Migrations, and Seed Data
├── Models/              # Domain entities (User, XP, Kana, Leaderboards)
├── Services/            # Business logic (XP calculations, Streak decay)
├── Views/               # Razor Templates (The presentation layer)
├── wwwroot/             # Static assets (Custom CSS, JS, Audio files)
└── NipponQuest.csproj   # Project configuration & NuGet dependencies

```

## ⚙️ Setup & Installation

To run this project locally, follow these steps:

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB

### Installation Steps

1. **Clone the Repository:**
```bash
git clone [https://github.com/marclourens19/NipponQuest.git](https://github.com/marclourens19/NipponQuest.git)

```


2. **Navigate to Project Root:**
```bash
cd NipponQuest/NipponQuest-main

```


3. **Configure Database:**
Update the `DefaultConnection` string in `appsettings.json` to point to your local SQL Server instance.
4. **Apply Migrations:**
```bash
dotnet ef database update

```


5. **Run Application:**
```bash
dotnet run

```


Visit `https://localhost:7000` in your browser.

## 🎓 Academic Context

This project was developed by **Joshua Marc Lourens** as a student in the **Bachelor of Computer Applications (BCA)** program. It highlights proficiency in:

* Implementing **CRUD** operations with Entity Framework.
* Applying **Dependency Injection** (DI) for decoupled service layers.
* Designing **Relational Databases** with complex user-data associations.
* Integrating **Middleware** for custom login behaviors (e.g., Streak tracking).

---

*NipponQuest is an ongoing project focused on merging educational psychology with software engineering best practices.*

