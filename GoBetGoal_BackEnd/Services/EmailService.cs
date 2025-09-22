using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace GoBetGoal_BackEnd.Services
{
    public class EmailService
    {
        /// <summary>
        /// 非同步地寄送密碼重設「連結」郵件
        /// </summary>
        public async Task SendPasswordResetLinkEmailAsync(string toEmail, string resetLink, string nickname)
        {

            // 1. 從設定檔中讀取寄件人信箱和應用程式密碼
            string fromEmail = WebConfigurationManager.AppSettings["GmailFromEmail"];
            string appPassword = WebConfigurationManager.AppSettings["GmailAppPassword"];

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "GoBetGoal 團隊"), // 第二個參數是「寄件人名稱」
                To = { new MailAddress(toEmail) },
                Subject = "【GoBetGoal】密碼重設請求",
                Body = $@"
                <html>
                <body>
                    <p>會員 {nickname} 您好，</p>
                    <p>我們已收到您的密碼重設請求。請點擊下方的連結來設定您的新密碼。此連結將在 15 分鐘後失效。</p>
                    <p><a href='{resetLink}'>點此重設您的密碼</a></p>
                    <p>如果您沒有請求重設密碼，請忽略此郵件。</p>
                    <p>GoBetGoal 團隊 敬上</p>
                </body>
                </html>",
                IsBodyHtml = true
            };

            // 1. 建立 SmtpClient，但這次我們手動設定
            using (var smtpClient = new SmtpClient())
            {
                // 3. 手動設定 SmtpClient 的屬性
                //    (即使 Web.config 有設定，這裡的設定會覆蓋它，讓行為更明確)
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                // *** 核心修改點：使用讀取到的金鑰來建立憑證 ***
                smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);

                // 4. 寄送郵件
                await smtpClient.SendMailAsync(mailMessage);

            }
        }
    }
}