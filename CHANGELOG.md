# Changelog

All notable changes to Timeline IQ will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-18

### 🎉 Initial Release

#### Added
- **Core Features**
  - Project time tracking with estimated vs. actual hours
  - SQLite database for local data storage
  - MVVM architecture with clean separation of concerns

- **Analytics Dashboard**
  - Total projects counter
  - Accuracy score calculation (0-100 scale)
  - Average deviation percentage
  - Under/overestimation counters

- **Estimation Bias Analysis**
  - Three-tier project size categorization (Small/Medium/Large)
  - Detailed breakdown by project size
  - Actionable insights for improvement

- **Smart Features**
  - Real-time prediction system based on historical data
  - Automatic status updates (Planned → Active → Completed)
  - Input validation (prevents zero-hour estimates)

- **Visualization**
  - Mini performance chart (last 5 completed projects)
  - Color-coded budget indicators (green/red)
  - Estimated vs. Actual comparison

- **Data Management**
  - Project filtering by status (All/Planned/Active/Completed)
  - Sortable DataGrid columns
  - CSV export functionality
  - Monthly report generation (.txt format)

- **Modern UI**
  - Custom dark theme (#1E1E1E background)
  - Card-based dashboard layout
  - Custom title bar (minimize/maximize/close)
  - Cyan (#4FC3F7) and purple (#7C4DFF) accent colors
  - Smooth hover effects and transitions

- **Technical**
  - .NET 8 WPF application
  - Single-file executable publishing
  - No external UI dependencies (pure WPF)
  - Programmatic icon loading (BAML-safe)

#### Known Issues
- ComboBox/DatePicker dropdowns use default Windows styling in published build
- Icon must be in same directory as executable

---

## [Unreleased]

### Planned Features
- PDF report generation
- Project categories/tags
- Team collaboration
- Cloud sync support
- Advanced charting library integration
- Dark/Light theme toggle
- Multi-language support (i18n)

---

**Legend:**
- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for now removed features
- `Fixed` for any bug fixes
- `Security` for vulnerability fixes
