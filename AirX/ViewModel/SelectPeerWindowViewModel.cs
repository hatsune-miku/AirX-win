﻿using AirX.Bridge;
using AirX.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.ViewModel
{
    public partial class SelectPeerWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public List<PeerItem> peers = new();
    }
}
