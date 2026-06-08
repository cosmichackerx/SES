# AI Based Smart Education System 🎓

![Smart Education System](https://img.shields.io/badge/Status-Active-brightgreen.svg) ![WinForms](https://img.shields.io/badge/Platform-Windows%20Forms-blue.svg) ![Database](https://img.shields.io/badge/Database-MySQL%20%7C%20JSON-orange.svg)

**AI Based Smart Education System** is a modern, high-performance desktop application built using C# Windows Forms (.NET 8) and Microsoft Edge WebView2. It aims to revolutionize local school and university management by providing an intuitive UI, lightning-fast database performance, and built-in AI assistance features.

**Developed by: `cosmichackerx`**

---

## 🌟 Key Features

- **Dynamic Database Switching**: Seamlessly toggle between a live **MySQL** server or a fallback local **JSON** data store without needing to restart the application.
- **Role-Based Access Control**: Secure login and signup mechanics separating `Teacher` and `Student` accounts. Critical sections like *Access Management* and *Attendance* modification are restricted strictly to teachers.
- **Modern UI Architecture**: Uses Microsoft WebView2 to render fluid, hardware-accelerated HTML/CSS/JS frontend interfaces directly inside a WinForms desktop shell.
- **Performance Optimized**: Zero-lag login/registration flows, asynchronous database calls, and ultra-fast application state transitions.
- **Standalone Executable**: Available as a completely self-contained `.exe` built with .NET 8, requiring absolutely no local frameworks or dependencies to run out-of-the-box.

---

## 🛠️ Technology Stack

- **Backend Logic**: C# (.NET 8.0)
- **Frontend UI**: HTML5, CSS3, JavaScript (rendered via Microsoft WebView2)
- **Database Backend**: MySQL (via `MySql.Data`)
- **Local Fallback**: JSON File Serialization (`System.Text.Json`)
- **Version Control**: Git / Git LFS

---

## 🚀 Getting Started

### Option 1: Run the Standalone App
1. Go to the `Exe` directory in this repository.
2. Double-click on `SmartEducationSystem.exe`. 
3. The app is completely self-contained and will run natively without requiring any .NET installations.
*(Note: If you are pulling this repo, ensure you have Git LFS installed to fetch the full 155MB `.exe` file)*

### Option 2: Build from Source
1. Open `SmartEducationSystem.sln` or `SmartEducationSystem.csproj` in **Visual Studio 2022**.
2. Make sure you have the **.NET 8.0 Desktop Development** workload installed.
3. Build and Run the project (`F5`).

---

## 🗄️ Database Configuration

By default, the application runs in **JSON mode** so it works instantly on any machine.
If you would like to test the application with a remote or local SQL server:
1. Login to the application.
2. Navigate to the **Test DBMS** tile.
3. Switch the Storage Mode to `MySQL`.
4. Enter your credentials and database name, then click **Save & Apply Configuration**.

---

## 🛡️ License & Credits

Developed independently by **[cosmichackerx](https://github.com/cosmichackerx)**.
This project was designed for the 3rd Semester Advanced Programming module.
