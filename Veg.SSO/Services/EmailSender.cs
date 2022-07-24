using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Veg.SSO.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("EMAIL:");
            Console.WriteLine(email);
            Console.WriteLine(subject);
            Console.WriteLine(htmlMessage);
            //Willemieke1!
            //no-reply@todo.com
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("hendrikdejonge@hotmail.com");
            mailMessage.To.Add(email);
            mailMessage.Body = htmlMessage;
            mailMessage.Subject = subject;
            SendEmail(subject, "ToDo", new string[] { email }, htmlMessage.Replace(Environment.NewLine, "<br/>"), null);
        }

        public void SendEmail(string subject, string fromName, string[] msgTo, string html, string text)
        {
            MailMessage mail = new MailMessage();

            //set the addresses 
            mail.From = new MailAddress("TODO", fromName); //IMPORTANT: This must be same as your smtp authentication address.
            foreach (var to in msgTo)
            {
                mail.To.Add(to);
            }

            //set the content 
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = html;
            //send the message 
            SmtpClient smtp = new SmtpClient("TODO");

            //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
            NetworkCredential Credentials = new NetworkCredential("TODO", "TODO");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 8889;    //alternative port number is 8889
            smtp.EnableSsl = false;
            smtp.Send(mail);
        }

        public async static Task<ElasticEmailClient.ApiTypes.EmailSend> SendEmailASync(string subject, string fromEmail, string fromName, string[] msgTo, string html, string text)
        {
            try
            {
                ElasticEmailClient.Api.ApiKey = "7e590acc-c2e0-4233-86f7-3c3990e480c2";
                return await ElasticEmailClient.Api.Email.SendAsync(subject, fromEmail, fromName, msgTo: msgTo, bodyHtml: html, bodyText: text);
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    Console.WriteLine("Server didn't accept the request: " + ex.Message);
                else
                    Console.WriteLine("Something unexpected happened: " + ex.Message);

                return null;
            }
        }
    }
}
