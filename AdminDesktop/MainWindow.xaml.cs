using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminDesktop.Models;
using AdminDesktop.ViewModels;

namespace AdminDesktop;

public partial class MainWindow : Window
{
    private MainViewModel VM => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    // === Login ===
    private void LoginBtn_Click(object sender, RoutedEventArgs e)
    {
        VM.LoginPassword = PwdBox.Password;
        if (VM.LoginCommand.CanExecute(null))
            VM.LoginCommand.Execute(null);
    }

    private void PwdBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        VM.LoginPassword = PwdBox.Password;
        if (VM.LoginCommand.CanExecute(null))
            VM.LoginCommand.Execute(null);
    }

    // === Navigation ===
    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && int.TryParse(rb.Tag?.ToString(), out var tab))
            VM.SelectedTab = tab;
    }

    // === Movies ===
    private void EditMovie_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is AdminMovie movie)
            VM.DoEditMovie(movie);
    }

    private async void DeleteMovie_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.DeleteMovieAsync(id);
    }

    private void ExtractVkUrl_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Window
        {
            Title = "Извлечь VK Video URL из iframe",
            Width = 500, Height = 280,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            Background = (System.Windows.Media.Brush)FindResource("BgDarkBrush")
        };

        var sp = new StackPanel { Margin = new Thickness(20) };
        sp.Children.Add(new TextBlock
        {
            Text = "Вставьте <iframe> код с VK Видео:",
            Foreground = (System.Windows.Media.Brush)FindResource("TextBrush"),
            Margin = new Thickness(0, 0, 0, 8)
        });

        var tb = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 100,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        sp.Children.Add(tb);

        var btn = new Button
        {
            Content = "Извлечь и вставить",
            Style = (Style)FindResource("BtnPrimary"),
            Margin = new Thickness(0, 12, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        btn.Click += (_, _) =>
        {
            VM.ExtractVkUrl(tb.Text);
            dlg.Close();
        };
        sp.Children.Add(btn);

        dlg.Content = sp;
        dlg.ShowDialog();
    }

    // === Users ===
    private async void MakeAdmin_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.ChangeRoleAsync(id, "admin");
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.DeleteUserAsync(id);
    }

    // === Content ===
    private async void DeleteGenre_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.DeleteGenreAsync(id);
    }

    private async void ToggleComment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.ToggleCommentAsync(id);
    }

    private async void DeleteComment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is long id)
            await VM.DeleteCommentAsync(id);
    }
}