using Microsoft.JSInterop;

namespace LANdalf.UI.Services;

public class ViewPreferenceService {
    private string currentView = "table";

    public event Action? OnViewChanged;

    public ViewPreferenceService() {
        currentView = "table";
    }

    public string CurrentView => currentView;

    public bool IsCardView => currentView == "card";

    public bool IsTableView => currentView == "table";

    public async Task LoadViewFromStorage(IJSRuntime jsRuntime) {
        try {
            var savedView = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "view-preference");
            currentView = savedView == "card" ? "card" : "table";
            OnViewChanged?.Invoke();
        } catch {
            // Fallback on localStorage error
            currentView = "table";
        }
    }

    public async Task SetView(IJSRuntime jsRuntime, string view) {
        if (view != "card" && view != "table") {
            throw new ArgumentException("View must be 'card' or 'table'", nameof(view));
        }

        currentView = view;
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", "view-preference", view);
        OnViewChanged?.Invoke();
    }

    public async Task ToggleView(IJSRuntime jsRuntime) {
        currentView = currentView == "card" ? "table" : "card";
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", "view-preference", currentView);
        OnViewChanged?.Invoke();
    }
}
