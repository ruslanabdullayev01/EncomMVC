using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Encom.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Globalization;
using System.Text;

namespace Encom.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly SmtpSetting _smtpSetting;
        public ContactUsController(AppDbContext db, SmtpSetting smtpSetting)
        {
            _db = db;
            _smtpSetting = smtpSetting;
        }

        public async Task<IActionResult> Index()
        {
            ContactUsVM contactVM = new()
            {
                Contact = await _db.Contacts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
            };
            return View(contactVM);
        }

        #region Form
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class PreventDuplicateRequestAttribute : ActionFilterAttribute
        {
            public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.HttpContext.Request.Form.ContainsKey("__RequestVerificationToken"))
                {
                    await context.HttpContext.Session.LoadAsync();

                    var currentToken = context.HttpContext.Request.Form["__RequestVerificationToken"].ToString();
                    var lastToken = context.HttpContext.Session.GetString("LastProcessedToken");

                    if (lastToken == currentToken)
                    {
                        context.ModelState.AddModelError(string.Empty, "Looks like you accidentally submitted the same form twice.");
                    }
                    else
                    {
                        context.HttpContext.Session.SetString("LastProcessedToken", currentToken);
                        await context.HttpContext.Session.CommitAsync();
                    }
                }
                await next();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDuplicateRequest]
        public async Task<IActionResult> Index(ContactUsVM contactVM)
        {
            if (contactVM.Appeal == null) return BadRequest();

            contactVM = new ContactUsVM()
            {
                Contact = await _db.Contacts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name),
                Appeal = contactVM.Appeal
            };

            if (!ModelState.IsValid) return View("Index", contactVM);

            Appeal appeal = contactVM.Appeal;
            appeal.IsDeleted = false;
            appeal.CreatedAt = DateTime.UtcNow.AddHours(4);

            #region String Builder
            StringBuilder sb = new StringBuilder();

            sb.Append("<h1>Form</h1>");
            sb.Append("<p><strong>Name: </strong>" + contactVM.Appeal.Name + "</p>");
            sb.Append("<p><strong>Surname: </strong>" + contactVM.Appeal.Surname + "</p>");
            sb.Append("<p><strong>Email: </strong>" + contactVM.Appeal.Email + "</p>");
            sb.Append("<p><strong>Phone Number: </strong>" + contactVM.Appeal.Phone + "</p>");
            sb.Append("<p><strong>Message: </strong>" + contactVM.Appeal.Message + "</p>");
            sb.Append("<p><strong>Send At: </strong>" + contactVM.Appeal.CreatedAt.ToString() + "</p>");

            string htmlTable = sb.ToString();
            #endregion

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(_smtpSetting.Email)); //UNDONE: Email appsettings de qeyd olunmalidir
            mimeMessage.Subject = contactVM.Appeal.Email;
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlTable;
            mimeMessage.Body = new TextPart("html")
            {
                Text = bodyBuilder.HtmlBody
            };

            using (SmtpClient smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }

            await _db.Appeals.AddAsync(appeal);
            await _db.SaveChangesAsync();

            string referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index), "ContactUs");
        }
        #endregion
    }
}
