namespace FinanceManagerAspNet.Navigation;

public sealed record FinanceSidebarViewModel(
    IReadOnlyList<FinanceDestination> Destinations,
    string? ActiveDestination,
    bool TasksActive,
    bool SettingsActive,
    bool IsLoggedIn,
    int DueTaskCount);
