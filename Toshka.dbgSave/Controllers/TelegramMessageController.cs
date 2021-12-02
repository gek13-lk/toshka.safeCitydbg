using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Toshka.dbgSave.DataAccess;
using Toshka.dbgSave.Model;
using Toshka.dbgSave.Services;
using Toshka.dbgSave.Services.Abstraction;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TelegramMessageController : ControllerBase
    {
        private readonly EfContext _context;
        private readonly IBotService _botService;
        public TelegramMessageController(EfContext context, IBotService botService)
        {
            _botService = botService;
            _context = context;
        }

        [HttpGet]
        public string Get()
        {
            return "Method GET unuvalable";
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            Update update
            )
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                string chatId = update.Message.Chat.Id.ToString();

                TelegramUser user = _context.TelegramUsers.Where(e => e.ChatId == chatId).Single();

                if (update.Message.Text == "/create_event")
                {
                    _botService.Client.SendTextMessageAsync(update.Message.From.Id, @$"Ответ create event").GetAwaiter().GetResult();
                }

                if (update.Message.Text == "/get_notifications_all" ||
                    update.Message.Text == "/get_notifications_graffiti")
                {
                    if (user == null)
                    {
                        user = new TelegramUser();
                        user.ChatId = update.Message.Chat.Id.ToString();
                        _context.TelegramUsers.Add(user);
                        _context.SaveChanges();
                        _botService.Client.SendTextMessageAsync(update.Message.From.Id, @$"Вы добавлены в наблюдатели").GetAwaiter().GetResult();
                    }

                    _botService.Client.SendTextMessageAsync(update.Message.From.Id, @$"Вы уже есть в наблюдателях").GetAwaiter().GetResult();
                }

                if (update.Message.Text == "/unset")
                {
                    if (user != null)
                    {
                        _context.TelegramUsers.Remove(user);
                        _context.SaveChanges();
                        _botService.Client.SendTextMessageAsync(update.Message.From.Id, @$"Вы удалены из наблюдателей").GetAwaiter().GetResult();
                    }
                }

                if (update.Message.Text == "/phones")
                {
                    _botService.Client.SendTextMessageAsync(update.Message.From.Id, @$"Удалены из наблюдателей").GetAwaiter().GetResult();
                }
            }

            return Ok();
        }

        /*[HttpPost]
        public async Task<OkResult> Post([FromBody] Update update)
        {
            if (update == null) return Ok();

            var commands = BotAsyncService.Commands;
            var message = update.Message;
            var botClient = await BotAsyncService.GetBotClientAsync();

            foreach (var command in commands)
            {
                if (command.Contains(message))
                {
                    await command.Execute(message, botClient);
                    break;
                }
            }

            return Ok();
        }*/
    }
}
