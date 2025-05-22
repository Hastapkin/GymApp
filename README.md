# 🏋️ Gym Management System

<div align="center">

![GymApp Logo](https://img.shields.io/badge/GymApp-v1.0.0-blue?style=for-the-badge&logo=dumbbell)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-XAML-orange?style=for-the-badge&logo=microsoft)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Oracle](https://img.shields.io/badge/Oracle-23ai-red?style=for-the-badge&logo=oracle)](https://www.oracle.com/)

**A modern, professional gym management application built with WPF .NET 8 and Oracle Database.**

[📖 Documentation](#-features) • [🚀 Quick Start](#-getting-started) • [📁 Structure](#-project-structure) • [🤝 Contributing](#-contributing)

</div>

---

## ✨ Features

| Feature | Description | Status |
|---------|-------------|--------|
| 👥 **Member Management** | Complete CRUD operations for gym members | ✅ |
| 🎫 **Membership Cards** | Track subscriptions and expiry dates | ✅ |
| 👷 **Staff Management** | Employee information and salary tracking | ✅ |
| 🔍 **Smart Search** | Real-time search across all modules | ✅ |
| 📊 **Dashboard** | Quick statistics and overview | ✅ |
| 🎨 **Modern UI** | Material Design with smooth animations | ✅ |

## 🛠️ Tech Stack

<table>
<tr>
<td valign="top" width="50%">

### Frontend
- **Framework**: WPF .NET 8
- **UI**: XAML, Material Design
- **Pattern**: MVVM Architecture
- **Styling**: Custom CSS-like styles
- **Animations**: Smooth transitions

</td>
<td valign="top" width="50%">

### Backend
- **Language**: C# 12
- **Database**: Oracle 23ai Free
- **ORM**: Oracle.ManagedDataAccess
- **Config**: JSON-based settings
- **Architecture**: Clean Architecture

</td>
</tr>
</table>

## 🚀 Getting Started

### 📋 Prerequisites

Before you begin, ensure you have met the following requirements:

- **Visual Studio 2022** or later
- **.NET 8 SDK** or later
- **Oracle Database 23ai Free**
- **Git** (for version control)

GymApp/
│
├── 📄 .gitignore                    # Git ignore rules
├── 📄 .gitattributes               # Git attributes
├── 📄 README.md                    # Project documentation
├── 📄 GymApp.csproj                # Project configuration
├── 📄 appsettings.json             # App configuration
├── 📄 App.xaml                     # Application entry point
├── 📄 App.xaml.cs                  # Application code-behind
├── 📄 AssemblyInfo.cs              # Assembly information
│
├── 📁 Commands/
│   └── 📄 RelayCommand.cs          # Command pattern implementation
│
├── 📁 Helpers/
│   └── 📄 Converters.cs            # XAML value converters
│       ├── NameToInitialsConverter
│       ├── StatusToColorConverter
│       ├── GenderToColorConverter
│       └── RoleToColorConverter
│
├── 📁 Models/
│   ├── 📄 Member.cs                # Member data model
│   ├── 📄 MembershipCard.cs        # Membership card model
│   └── 📄 Staff.cs                 # Staff data model
│
├── 📁 Services/
│   ├── 📄 ConfigurationService.cs  # Configuration management
│   └── 📄 DatabaseService.cs       # Database operations
│
├── 📁 ViewModels/
│   ├── 📄 BaseViewModel.cs         # Base ViewModel class
│   ├── 📄 MainViewModel.cs         # Main window ViewModel
│   ├── 📄 MembersViewModel.cs      # Members page ViewModel
│   ├── 📄 MembershipCardsViewModel.cs # Cards page ViewModel
│   └── 📄 StaffViewModel.cs        # Staff page ViewModel
│
├── 📁 Views/
│   ├── 📄 LoginWindow.xaml + .cs   # Login interface
│   ├── 📄 MainWindow.xaml + .cs    # Main application window
│   ├── 📄 MembersPage.xaml + .cs   # Members management page
│   ├── 📄 MembershipCardsPage.xaml + .cs # Cards management page
│   └── 📄 StaffPage.xaml + .cs     # Staff management page
│
└── 📁 Dependencies/
    └── 📦 NuGet Packages
        ├── Oracle.ManagedDataAccess.Core (23.4.0)
        └── Newtonsoft.Json (13.0.3)
        
