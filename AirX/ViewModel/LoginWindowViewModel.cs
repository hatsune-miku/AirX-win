using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirX.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AirX
{
    public partial class LoginWindowViewModel: ObservableObject
    {
        [ObservableProperty]
        public string buttonTitle = "ConfirmSignIn".Text();

        [ObservableProperty]
        public string uid = "";

        [ObservableProperty]
        public string password = "";

        [ObservableProperty]
        public bool shouldRememberPassword = true;

        [ObservableProperty]
        public bool isLoggingIn = false;

        [ObservableProperty]
        public string controlPanelTitle = "AirXControlPanel".Text();

        [ObservableProperty]
        public string textUidOrEmail = "UidOrEmail".Text();

        [ObservableProperty]
        public string textPassword = "Password".Text();

        [ObservableProperty]
        public string textSignIn = "SignIn".Text();

        [ObservableProperty]
        public string textRememberMe = "RememberMe".Text();

        [ObservableProperty]
        public string textConfirmSignIn = "ConfirmSignIn".Text();

        [ObservableProperty]
        public string textSignUp = "SignUp".Text();

        [ObservableProperty]
        public string textCreateOne = "CreateOne".Text();

        [ObservableProperty]
        public string textNoAirXAccountYet = "NoAirXAccountYet".Text();
    }
}
