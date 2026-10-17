# WindowStateManager

WindowStateManager is a lightweight and DPI-aware window state utility for WinUI 3 applications.  
It automatically saves and restores the window's size, position, and maximized state using `ApplicationData.Current.LocalSettings`.  
You can also lock the window size by specifying fixed dimensions through the `Attach` overloads.

## Features

- Save and restore window size and position
- Restore maximized state
- Optional fixed-size mode (DPI-aware)
- Uses WinUI 3 `AppWindow` API
- Simple integration with a single `Attach` call
- No external dependencies

## Usage

Add the following line to your `MainWindow` constructor:

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Add
        WindowManager.Attach(this); 
    }
}
```