using AirX.ViewModel;
using CommunityToolkit.Labs.WinUI;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace AirX.Pages
{
    public sealed partial class SelectPeerPage : Page
    {
        private SelectPeerWindowViewModel ViewModel = new();

        public delegate void OnPeerSelectedHandler(Model.PeerItem peer);

        public event OnPeerSelectedHandler OnPeerSelected;

        public SelectPeerPage()
        {
            this.InitializeComponent();

            ViewModel.Peers.AddRange(
                AirXBridge.GetPeers()
                .Select(peer => new Model.PeerItem(peer))
            );
        }

        private void OnSettingsCardClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!(sender is SettingsCard card))
            {
                return;
            }
            var peers = ViewModel.Peers
                .Where(peer => peer.GetDescription() == card.Description as string)
                .ToList();
            if (peers.Any())
            {
                OnPeerSelected(peers.First());
            }
        }
    }
}
