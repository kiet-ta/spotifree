using Spotifree.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.Services
{
    public class ConnectivityService : IConnectivityService
    {
        public bool IsConnected()
        {
            //check NIC (card network are really?)
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
}
