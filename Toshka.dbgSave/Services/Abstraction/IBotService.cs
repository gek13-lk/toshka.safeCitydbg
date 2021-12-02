using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Toshka.dbgSave.Services.Abstraction
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}