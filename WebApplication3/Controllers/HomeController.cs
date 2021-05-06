using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {

            try
            {

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse("SENDER_EMAIL_ACCOUNT"));
                mimeMessage.To.Add(MailboxAddress.Parse("RECEIVER_EMAIL_ACCOUNT"));
                mimeMessage.Subject = "EMAIL_Subject";

                mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
                { Text = "EMAIL_BODY" };



                using (SmtpClient smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("smtp.gmail.com", 465, true).ConfigureAwait(false);
                    await smtpClient.AuthenticateAsync("YOUR_EMAIL_ACCOUNT", "YOUR_EMAIL_SECRET").ConfigureAwait(false);
                    await smtpClient.SendAsync(mimeMessage).ConfigureAwait(false);
                    await smtpClient.DisconnectAsync(true).ConfigureAwait(false);
                }

            }
            catch (SmtpCommandException ex)
            {

                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        _logger.LogError(ex, $"\tRecipient not accepted: {ex.Mailbox}");
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        _logger.LogError(ex, $"\tSender not accepted: {ex.Mailbox}");
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        _logger.LogError(ex, $"\tMessage not accepted.");
                        break;
                }

            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}