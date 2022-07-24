using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Veg.API.Client;

namespace Veg.Admin.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow, INotifyPropertyChanged
    {
        private LoginRepository _loginRepository;
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;

            _loginRepository = App.Container.GetInstance<LoginRepository>();
            ThemeManager.Current.ChangeTheme(this, "Light.Green");
            LoginCommand = new AsyncCommand(async (_) =>
            {
                if (!string.IsNullOrWhiteSpace(PasswordTxt.Password) && !string.IsNullOrWhiteSpace(UsernameTxt.Text))
                {
                    var result = await _loginRepository.Login(UsernameTxt.Text, PasswordTxt.Password, App.Container.GetInstance<MembersClient>());
                    if (result.IsError)
                    {
                        switch (result.ErrorType)
                        {
                            case LoginErrorType.NoConnection:
                                ErrorMessage = "Geen verbinding";
                                break;
                            default:
                                ErrorMessage = "Geen gebruiker met deze inloggegevens";
                                break;
                        }
                    }
                    else
                    {
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                }
            },
            () =>
            {
                return true;
            });
        }

        string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        bool _tryingLogin;
        public bool TryingLogin
        {
            get
            {
                return _tryingLogin;
            }
            set
            {
                _tryingLogin = value;
                OnPropertyChanged();
            }
        }

        AsyncCommand _loginCommand;
        public AsyncCommand LoginCommand
        {
            get
            {
                return _loginCommand;
            }
            set
            {
                _loginCommand = value;
                OnPropertyChanged();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null, bool fileChanged = false)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UsernameTxt.Text = "hendrikdejonge@hotmail.com";
            PasswordTxt.Password = "Dejonge1";
            LoginCommand.ExecuteAsync(null).FireAndForgetSafeAsync();
        }
    }
}
