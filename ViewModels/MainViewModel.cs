using ProjectTimeEstimator.Models;
using ProjectTimeEstimator.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace ProjectTimeEstimator.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    public MainViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Projects = new ObservableCollection<Project>();
        
        AddProjectCommand = new RelayCommand(AddProject, CanAddProject);
        UpdateProjectCommand = new RelayCommand(UpdateProject, CanUpdateProject);
        DeleteProjectCommand = new RelayCommand(DeleteProject, CanDeleteProject);
        ClearFormCommand = new RelayCommand(ClearForm);
        ExportCommand = new RelayCommand(ExportToCsv);
        MonthlyReportCommand = new RelayCommand(ExportMonthlyReport);

        StartDate = DateTime.Today;
        Title = "Timeline IQ";

        LoadProjects();
    }

    #region Properties

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public ObservableCollection<Project> Projects { get; }

    private Project? _selectedProject;
    public Project? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetField(ref _selectedProject, value) && value != null)
            {
                PopulateForm(value);
            }
        }
    }

    private int _id;
    private string _name = string.Empty;
    public string Name { get => _name; set => SetField(ref _name, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetField(ref _description, value); }

    private double _estimatedHours;
    public double EstimatedHours { get => _estimatedHours; set { if(SetField(ref _estimatedHours, value)) UpdatePrediction(); } }

    private double _actualHours;
    public double ActualHours { get => _actualHours; set => SetField(ref _actualHours, value); }

    private DateTime _startDate;
    public DateTime StartDate { get => _startDate; set => SetField(ref _startDate, value); }

    private DateTime? _endDate;
    public DateTime? EndDate { get => _endDate; set => SetField(ref _endDate, value); }

    private string _status = "Planned";
    public string Status { get => _status; set => SetField(ref _status, value); }

    public List<string> StatusOptions { get; } = new List<string> { "Planned", "Active", "Completed" };

    public int TotalProjectsCount => Projects.Count;
    public int CompletedProjectsCount => Projects.Count(p => p.Status == "Completed");
    public double AverageDeviation
    {
        get
        {
            var validProjects = Projects.Where(p => p.EstimatedHours > 0).ToList();
            if (validProjects.Count == 0) return 0;
            return validProjects.Average(p => p.PercentageDeviation);
        }
    }

    public double AverageAccuracy
    {
        get
        {
            var completed = Projects.Where(p => p.Status == "Completed" && p.EstimatedHours > 0).ToList();
            if (completed.Count == 0) return 0;
            return completed.Average(p => p.AccuracyScore);
        }
    }

    public int OverestimatedCount => Projects.Count(p => p.IsOverestimated);
    public int UnderestimatedCount => Projects.Count(p => p.IsUnderestimated);

    // Analysis Text
    private string _biasAnalysisText = string.Empty;
    public string BiasAnalysisText
    {
        get => _biasAnalysisText;
        set => SetField(ref _biasAnalysisText, value);
    }

    // Detailed Bias Analysis
    private string _biasAnalysisDetails = string.Empty;
    public string BiasAnalysisDetails
    {
        get => _biasAnalysisDetails;
        set => SetField(ref _biasAnalysisDetails, value);
    }

    // Recommendation
    private string _predictionText = "Veri bekleniyor...";
    public string PredictionText
    {
        get => _predictionText;
        set => SetField(ref _predictionText, value);
    }

    // Filter
    private string _filterStatus = "All";
    public string FilterStatus
    {
        get => _filterStatus;
        set
        {
            if (SetField(ref _filterStatus, value))
            {
                ApplyFilter();
            }
        }
    }
    public List<string> FilterOptions { get; } = new List<string> { "All", "Planned", "Active", "Completed" };

    // Display Collection
    private ObservableCollection<Project> _filteredProjects;
    public ObservableCollection<Project> FilteredProjects 
    {
        get => _filteredProjects;
        set => SetField(ref _filteredProjects, value);
    }

    // Chart Data (Simple Bindings)
    public List<BasicBarData> ChartData { get; private set; } = new();

    #endregion

    #region Commands

    public ICommand AddProjectCommand { get; }
    public ICommand UpdateProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand MonthlyReportCommand { get; }

    #endregion

    #region Methods

    private void LoadProjects()
    {
        try
        {
            Projects.Clear();
            var list = _databaseService.GetAllProjects();
            foreach (var project in list)
            {
                Projects.Add(project);
            }
            ApplyFilter();
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (FilterStatus == "All")
        {
            FilteredProjects = new ObservableCollection<Project>(Projects);
        }
        else
        {
            FilteredProjects = new ObservableCollection<Project>(Projects.Where(p => p.Status == FilterStatus));
        }
    }

    private void UpdateStatistics()
    {
        OnPropertyChanged(nameof(TotalProjectsCount));
        OnPropertyChanged(nameof(CompletedProjectsCount));
        OnPropertyChanged(nameof(AverageDeviation));
        OnPropertyChanged(nameof(AverageAccuracy));
        OnPropertyChanged(nameof(OverestimatedCount));
        OnPropertyChanged(nameof(UnderestimatedCount));

        // Detailed Bias Analysis by Size Category
        var smallProjects = Projects.Where(p => p.EstimatedHours > 0 && p.EstimatedHours <= 10).ToList();
        var mediumProjects = Projects.Where(p => p.EstimatedHours > 10 && p.EstimatedHours <= 50).ToList();
        var largeProjects = Projects.Where(p => p.EstimatedHours > 50).ToList();

        double smallDev = smallProjects.Count > 0 ? smallProjects.Average(p => p.PercentageDeviation) : 0;
        double mediumDev = mediumProjects.Count > 0 ? mediumProjects.Average(p => p.PercentageDeviation) : 0;
        double largeDev = largeProjects.Count > 0 ? largeProjects.Average(p => p.PercentageDeviation) : 0;

        // Simple summary
        string advice = "";
        if (Math.Abs(smallDev) > Math.Abs(largeDev) && Math.Abs(smallDev) > Math.Abs(mediumDev))
            advice = $"Küçük projelerde (%{smallDev:F1}) sapma en yüksek.";
        else if (Math.Abs(largeDev) > Math.Abs(smallDev) && Math.Abs(largeDev) > Math.Abs(mediumDev))
            advice = $"Büyük projelerde (%{largeDev:F1}) risk artıyor.";
        else if (mediumProjects.Count > 0)
            advice = $"Orta ölçekli projelerde (%{mediumDev:F1}) denge iyi.";
        else
            advice = "Daha fazla veri girişi yapın.";

        BiasAnalysisText = $"Analiz: {advice}";

        // Detailed breakdown
        var detailsBuilder = new System.Text.StringBuilder();
        detailsBuilder.AppendLine("📊 ESTIMATION BIAS ANALYSIS\n");
        detailsBuilder.AppendLine($"🔹 Small (0-10h): {smallProjects.Count} proje, Sapma: {smallDev:F1}%");
        detailsBuilder.AppendLine($"🔹 Medium (10-50h): {mediumProjects.Count} proje, Sapma: {mediumDev:F1}%");
        detailsBuilder.AppendLine($"🔹 Large (50h+): {largeProjects.Count} proje, Sapma: {largeDev:F1}%\n");
        
        // Find worst category
        var maxDev = new[] { ("Small", Math.Abs(smallDev)), ("Medium", Math.Abs(mediumDev)), ("Large", Math.Abs(largeDev)) }
                        .OrderByDescending(x => x.Item2).First();
        detailsBuilder.AppendLine($"⚠️ En riskli kategori: {maxDev.Item1} ({maxDev.Item2:F1}% sapma)");
        
        BiasAnalysisDetails = detailsBuilder.ToString();

        // Chart Data Update (Mocking a simple data structure for UI)
        ChartData = Projects.Where(p => p.Status == "Completed")
                            .OrderByDescending(p => p.StartDate)
                            .Take(5)
                            .Select(p => new BasicBarData 
                            { 
                                Label = p.Name, 
                                Value1 = p.EstimatedHours,
                                Value2 = p.ActualHours
                            }).ToList();
        OnPropertyChanged(nameof(ChartData));
    }

    private void UpdatePrediction()
    {
        if (EstimatedHours <= 0) 
        {
             PredictionText = "Süre girin...";
             return;
        }

        // Simple Prediction: Global Avg Deviation multiplier
        double avgDev = AverageDeviation; 
        double suggested = EstimatedHours * (1 + (avgDev / 100.0));
        
        PredictionText = $"Öneri: {suggested:F1} saat (Genel sapma: %{avgDev:F1})";
    }

    private void ExportToCsv(object? parameter)
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TimelineIQ_Report.csv");
            using var writer = new StreamWriter(path);
            writer.WriteLine("Id,Name,Description,Estimated,Actual,Deviation,Status,Date");
            foreach(var p in Projects)
            {
                writer.WriteLine($"{p.Id},{p.Name},{p.Description},{p.EstimatedHours},{p.ActualHours},{p.PercentageDeviation:F2}%,{p.Status},{p.StartDate:yyyy-MM-dd}");
            }
            MessageBox.Show($"Rapor kaydedildi: {path}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch(Exception ex)
        {
            MessageBox.Show($"Dışa aktarma hatası: {ex.Message}");
        }
    }

    private void ExportMonthlyReport(object? parameter)
    {
        try
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyProjects = Projects.Where(p => p.StartDate >= monthStart && p.StartDate <= monthEnd).ToList();

            if (monthlyProjects.Count == 0)
            {
                MessageBox.Show("Bu ay için proje bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var fileName = $"TimelineIQ_Monthly_{now:yyyy_MM}.txt";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            
            using var writer = new StreamWriter(path);
            writer.WriteLine("═══════════════════════════════════════════════════════");
            writer.WriteLine($"       TIMELINE IQ - AYLIK RAPOR ({now:MMMM yyyy})");
            writer.WriteLine("═══════════════════════════════════════════════════════\n");
            
            writer.WriteLine($"📅 Rapor Tarihi: {now:dd.MM.yyyy HH:mm}");
            writer.WriteLine($"📊 Toplam Proje: {monthlyProjects.Count}");
            writer.WriteLine($"✅ Tamamlanan: {monthlyProjects.Count(p => p.Status == "Completed")}");
            writer.WriteLine($"🔄 Aktif: {monthlyProjects.Count(p => p.Status == "Active")}");
            writer.WriteLine($"📝 Planlanan: {monthlyProjects.Count(p => p.Status == "Planned")}\n");

            var completed = monthlyProjects.Where(p => p.Status == "Completed" && p.EstimatedHours > 0).ToList();
            if (completed.Count > 0)
            {
                writer.WriteLine("───────────────────────────────────────────────────────");
                writer.WriteLine("📈 PERFORMANS ANALİZİ\n");
                writer.WriteLine($"Ortalama Sapma: {completed.Average(p => p.PercentageDeviation):F1}%");
                writer.WriteLine($"Doğruluk Skoru: {completed.Average(p => p.AccuracyScore):F1}/100");
                writer.WriteLine($"Düşük Tahmin: {completed.Count(p => p.IsUnderestimated)} proje");
                writer.WriteLine($"Yüksek Tahmin: {completed.Count(p => p.IsOverestimated)} proje\n");
            }

            // Bias Analysis for this month
            var smallMonth = monthlyProjects.Where(p => p.EstimatedHours > 0 && p.EstimatedHours <= 10).ToList();
            var mediumMonth = monthlyProjects.Where(p => p.EstimatedHours > 10 && p.EstimatedHours <= 50).ToList();
            var largeMonth = monthlyProjects.Where(p => p.EstimatedHours > 50).ToList();

            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine("🎯 ESTIMATION BIAS ANALYSIS\n");
            writer.WriteLine($"Küçük Projeler (0-10h): {smallMonth.Count} adet");
            if (smallMonth.Count > 0)
                writer.WriteLine($"  → Ortalama Sapma: {smallMonth.Average(p => p.PercentageDeviation):F1}%");
            
            writer.WriteLine($"\nOrta Projeler (10-50h): {mediumMonth.Count} adet");
            if (mediumMonth.Count > 0)
                writer.WriteLine($"  → Ortalama Sapma: {mediumMonth.Average(p => p.PercentageDeviation):F1}%");
            
            writer.WriteLine($"\nBüyük Projeler (50h+): {largeMonth.Count} adet");
            if (largeMonth.Count > 0)
                writer.WriteLine($"  → Ortalama Sapma: {largeMonth.Average(p => p.PercentageDeviation):F1}%\n");

            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine("📋 PROJE LİSTESİ\n");
            foreach (var p in monthlyProjects.OrderBy(p => p.StartDate))
            {
                writer.WriteLine($"• {p.Name}");
                writer.WriteLine($"  Durum: {p.Status} | Tahmin: {p.EstimatedHours}h | Gerçek: {p.ActualHours}h");
                writer.WriteLine($"  Sapma: {p.PercentageDeviation:F1}% | Tarih: {p.StartDate:dd.MM.yyyy}\n");
            }

            writer.WriteLine("═══════════════════════════════════════════════════════");
            writer.WriteLine($"Rapor oluşturuldu: {now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine("═══════════════════════════════════════════════════════");

            MessageBox.Show($"Aylık rapor kaydedildi:\n{path}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Rapor oluşturma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PopulateForm(Project project)
    {
        _id = project.Id;
        Name = project.Name;
        Description = project.Description;
        EstimatedHours = project.EstimatedHours;
        ActualHours = project.ActualHours;
        StartDate = project.StartDate;
        EndDate = project.EndDate;
        Status = project.Status;
    }

    private void ClearForm(object? parameter = null)
    {
        SelectedProject = null;
        _id = 0;
        Name = string.Empty;
        Description = string.Empty;
        EstimatedHours = 0;
        ActualHours = 0;
        StartDate = DateTime.Today;
        EndDate = null;
        Status = "Planned";
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Proje adı boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (EstimatedHours < 0 || ActualHours < 0)
        {
            MessageBox.Show("Süreler negatif olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private bool CanAddProject(object? parameter) => true;

    private void AddProject(object? parameter)
    {
        if (!ValidateForm()) return;

        try
        {
            // Status Automation
            if (ActualHours > 0 && Status == "Planned") Status = "Active";
            if (EndDate.HasValue && Status != "Completed") Status = "Completed";

            var newProject = new Project
            {
                Name = Name,
                Description = Description,
                EstimatedHours = EstimatedHours,
                ActualHours = ActualHours,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = Status
            };

            _databaseService.AddProject(newProject);
            LoadProjects();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanUpdateProject(object? parameter) => SelectedProject != null;

    private void UpdateProject(object? parameter)
    {
        if (SelectedProject == null || !ValidateForm()) return;

        try
        {
            var projectToUpdate = new Project
            {
                Id = _id,
                Name = Name,
                Description = Description,
                EstimatedHours = EstimatedHours,
                ActualHours = ActualHours,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = Status
            };

            _databaseService.UpdateProject(projectToUpdate);
            LoadProjects();
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanDeleteProject(object? parameter) => SelectedProject != null;

    private void DeleteProject(object? parameter)
    {
        if (SelectedProject == null) return;

        var result = MessageBox.Show($"'{SelectedProject.Name}' projesini silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _databaseService.DeleteProject(SelectedProject.Id);
                LoadProjects();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}
