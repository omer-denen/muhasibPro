using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public enum SagaStepStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

public class SagaStepItem : INotifyPropertyChanged
{
    public SagaStepItem(int stepNumber, string name, string description)
    {
        StepNumber = stepNumber;
        Name = name;
        Description = description;
    }

    public int StepNumber { get; }
    public string Name { get; }
    public string Description { get; }

    private string _timestamp = string.Empty;
    public string Timestamp
    {
        get => _timestamp;
        set => Set(ref _timestamp, value);
    }

    private SagaStepStatus _status = SagaStepStatus.Pending;
    public SagaStepStatus Status
    {
        get => _status;
        set
        {
            if (Set(ref _status, value))
                OnPropertyChanged(nameof(StatusGlyph));
        }
    }

    public string StatusGlyph => Status switch
    {
        SagaStepStatus.Completed => "\uE73E",
        SagaStepStatus.Failed => "\uEA39",
        _ => StepNumber.ToString()
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
