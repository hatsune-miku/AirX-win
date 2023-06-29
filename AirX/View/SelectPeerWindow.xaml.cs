using AirX.Bridge;
using AirX.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace AirX.View
{
    public sealed partial class SelectPeerWindow : BaseWindow
    {
        private static SelectPeerWindow _instance = new();
        private PeerItem _selectedPeer = null;

        public static SelectPeerWindow Instance { get => _instance; }

        private SelectPeerWindow()
        {
            this.InitializeComponent();
        }

        public void OnPeerSelected(Model.PeerItem peer)
        {
            _selectedPeer = peer;
        }

        public async Task<List<Peer>> SelectPeersAsync()
        {
            Activate();
            while (_selectedPeer == null)
            {
                await Task.Delay(100);
            }

            // TODO: impl multiple peers selection
            Close();
            return new List<Peer> { _selectedPeer.Value };
        }
    }
}
