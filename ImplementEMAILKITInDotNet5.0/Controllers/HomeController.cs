using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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


            var a = new List<string>();
            a.Add("myong333@icloud.com");

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse("asdasdasdasdas@gmail.com"));

            foreach (var item in a)
            {
                mimeMessage.To.Add(MailboxAddress.Parse(item));

            }
            mimeMessage.Subject = "EMAIL_Subject";

            var builder = new BodyBuilder();
            builder.TextBody = "<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /></head><body leftmargin='0' topmargin='0' marginwidth='0' marginheight='0' width='700' ><table width='650px' border='0' cellpadding='0' cellspacing='0'><tr></tr></table><table width='650px' border='0' cellpadding='0' cellspacing='0' class='Context_tb' ><tr><td class='Context_td01' align='left'>親愛的顧客，您好：<br><br>您於網路郵局終止約定帳戶之申請已完成，該約定帳戶已失效。請詳閱附檔，檢視終止帳戶之資料。<br><br></td></tr><tr><td class='Context_td01' align='left'>提醒您，附檔已設定為加密文件，<font color='red'>開啟密碼為您的身分證字號(大寫英文字母+9位數字；外籍人士為居留證號碼：兩個大寫英文字母+8位數字)。</font><br><br></td></tr><tr><td class='Context_td01' align='left'>若您無法看到電子郵件通知單內容，請至Adobe Acrobat官網下載Adobe Reader軟體並安裝。<br><br></td></tr><tr><td class='Context_td01' align='left'>如需任何協助，請隨時致電本公司24小時客戶服務中心0800-700-365，手機請改撥付費電話(04)23542030。<br><br>本信件由系統發出，請勿直接回覆此信。</td></tr></td></tr></table></body></html>";
            var attachmentPath = @"D:\Yong-Wen Huang_CV(English).pdf";

            // We may also want to attach a calendar event for Monica's party...
            if (!System.IO.File.Exists(attachmentPath))
            {
                var aa = @"D:\Yong-Wen Huang_CV(English).pdf";
                return View();

            }
            builder.Attachments.Add(attachmentPath);

            // Now we just need to set the message body and we're done
            mimeMessage.Body = builder.ToMessageBody();

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("smtp.gmail.com", 465, true);
                    await smtpClient.AuthenticateAsync("veve0331@gmail.com", "rqnaqrtjjyygujyg");
                    await smtpClient.SendAsync(mimeMessage);
                    await smtpClient.DisconnectAsync(true);
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
                var b = await SendMailThread(mimeMessage, 0).ConfigureAwait(false);

            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");
                var b = await SendMailThread(mimeMessage, 0).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");
                var b = await SendMailThread(mimeMessage, 0).ConfigureAwait(false);
                var adfs = 0;
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Repeat Send Mail Thread
        /// </summary>
        /// <param name="message">mail message</param>
        /// <param name="count">已重新寄送次數</param>
        /// <returns>0:success; -1:fail</returns>
        private async Task<bool> SendMailThread(MimeMessage message, int count)
        {
            try
            {
                if (count >= 3)
                {
                    _logger.LogError($"EmailService-SendEmailAsync: (收件人:{message.To})共嘗試重新寄送信件3次皆失敗，停止重新寄送信件流程 SendMailThread Error");

                    return false;
                }

                _logger.LogInformation($"EmailService-SendEmailAsync: (收件人:{message.To})於2秒後重新寄信");
                var qq = $"EmailService-SendEmailAsync: (收件人:{message.To})於2秒後重新寄信";
                count++;

                await Task.Delay(2000);

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("smtp.gmail.com", 465, true).ConfigureAwait(false);
                    await smtpClient.AuthenticateAsync("veve0331@gmail.com", "rqnaqrtjjyygujyg").ConfigureAwait(false);
                    await smtpClient.SendAsync(message).ConfigureAwait(false);
                    await smtpClient.DisconnectAsync(true).ConfigureAwait(false);
                }

                _logger.LogInformation($"EmailService-SendEmailAsync: (收件人:{message.To}))於第{count}次重新寄送信件成功。");
                return true;
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError($"EmailService-SendEmailAsync: ((收件人:{message.To})第{count}次重新寄送失敗。");

                _logger.LogError(ex, $"Error sending message: {ex.Message}");
                _logger.LogError(ex, $"\tStatusCode: {ex.StatusCode}");

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

                return await SendMailThread(message, count).ConfigureAwait(false);

            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError($"EmailService-SendEmailAsync: ((收件人:{message.To})第{count}次重新寄送失敗。");
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");

                return await SendMailThread(message, count).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _logger.LogError($"EmailService-SendEmailAsync: ((收件人:{message.To})第{count}次重新寄送失敗。");
                _logger.LogError(ex, $"Protocol error while sending message: {ex.Message}");

                return await SendMailThread(message, count).ConfigureAwait(false);

            }
        }
    }
}