using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Telegram.Bot;
using Toshka.dbgSave.Services.Abstraction;
using System.Threading.Tasks;
using System.Linq;

namespace Toshka.dbgSave.Services
{
    public class BotService : IBotService
    {
        public BotService(IOptions<BotConfiguration> config)
        {
            Client = new TelegramBotClient(config.Value.BotToken);
        }

        public TelegramBotClient Client { get; }
    }
}