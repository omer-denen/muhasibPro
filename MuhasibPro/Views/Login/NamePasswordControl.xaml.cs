using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.Login;

public sealed partial class NamePasswordControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(LoginViewModel), typeof(NamePasswordControl), new PropertyMetadata(null));

    public LoginViewModel ViewModel
    {
        get => (LoginViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public NamePasswordControl()
    {
        this.InitializeComponent();
    }

    private bool _isPasswordVisible = false;

    private void OnRevealClick(object sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        if (_isPasswordVisible)
        {
            RevealIcon.Glyph = "\uE7B3";
            PasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
        }
        else
        {
            RevealIcon.Glyph = "\uE052";
            PasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
        }
    }

    private void OnRememberMeToggled(object sender, RoutedEventArgs e)
    {
        var isChecked = RememberMeCheckBox.IsChecked == true;
        var message = isChecked
            ? "Kullanıcı adınız hatırlanacak — şifre asla kaydedilmez"
            : "Kullanıcı adı hatırlanmayacak";
        RememberInfoBorder.ShowAsync(message);
    }
}
