using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Toshka.SafeCity.Hub
{
    public class DetectionHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public Task SendMessage(ImageMessage image)
        {
            return Clients.All.SendAsync("ondetected", image);
        }
    }

    public class ImageMessage
    {
        public string ImageBase64 { get; set; }
        public string ImageHeaders { get; set; }
    }
}