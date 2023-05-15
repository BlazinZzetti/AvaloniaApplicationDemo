using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AvaloniaApplicationDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        PlayButton.Click += OnPlayButtonPressed;
    }

    private void OnPlayButtonPressed(object? sender, RoutedEventArgs e)
    {
        PlayButton.Background = new SolidColorBrush(0xFFFFFF);
    }
}