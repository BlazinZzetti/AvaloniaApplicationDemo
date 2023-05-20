using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplicationDemo;

public partial class MessageBox : Window
{
    [Flags]
    public enum MessageOptions
    {
        None = 0,
        Yes = 1,
        Ok = 2,
        Cancel = 4,
        YesAndCancel = 5,
    }
    public MessageBox()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void SetupMessageBox(string message)
    {
        MessageTextBlock.Text = message;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}