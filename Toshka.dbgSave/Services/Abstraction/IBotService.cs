using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Toshka.SafeCity.Model.Command;

namespace Toshka.SafeCity.Services.Abstraction
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}