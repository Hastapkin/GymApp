# GymApp
WPF C# .NET8 
# GymApp
WPF C# .NET8 
GymApp/
├── GymApp.csproj
├── appsettings.json
├── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
│
├── Commands/
│   └── RelayCommand.cs
│
├── Helpers/
│   └── Converters.cs
│       ├── NameToInitialsConverter
│       ├── StatusToColorConverter
│       ├── GenderToColorConverter
│       └── RoleToColorConverter
│
├── Models/
│   ├── Member.cs
│   ├── MembershipCard.cs (+ Package class)
│   └── Staff.cs
│
├── Services/
│   ├── ConfigurationService.cs
│   └── DatabaseService.cs
│
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── MainViewModel.cs
│   ├── MembersViewModel.cs
│   ├── MembershipCardsViewModel.cs
│   └── StaffViewModel.cs
│
├── Views/
│   ├── LoginWindow.xaml + .cs
│   ├── MainWindow.xaml + .cs
│   ├── MembersPage.xaml + .cs
│   ├── MembershipCardsPage.xaml + .cs
│   └── StaffPage.xaml + .cs
│
└── Dependencies/
    └── Packages/
        ├── Oracle.ManagedDataAccess.Core (23.4.0)
        └── Newtonsoft.Json (13.0.3)
UI Architecture:
MainWindow (Navigation)
├── LoginWindow (Entry Point)
├── MembersPage (CRUD Members)
├── MembershipCardsPage (Manage Cards)
└── StaffPage (HR Management)
Database Schema:
Oracle Database (C##GymApp)
├── Members (Thành viên)
├── MembershipCards (Thẻ tập)
├── Packages (Gói tập)
├── Staff (Nhân sự)
└── CheckInLog (Lịch sử check-in)
