﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    public partial class SplitPageViewModel : ObservableObject
    {
        [ObservableProperty]
        bool shouldExpandPane = true;
    }
}
