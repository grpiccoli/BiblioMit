using BiblioMit.Extensions;
using BiblioMit.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace BiblioMit.Services
{
    public class FlowService : IFlow
    {
        private readonly FlowSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;
        private readonly string _os;
        public FlowService(IOptions<FlowSettings> settings,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper,
            IWebHostEnvironment environment)
        {
            _urlHelper = urlHelper;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            if (settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings.Value;
            _os = Environment.OSVersion.Platform.ToString();
        }
        public string FlowJs(string function, SortedDictionary<string, string> form)
        {
            string args = string.Join("&",
                form.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}={1}", kvp.Key, kvp.Value)));
            string script = Path.Combine(
                _environment.ContentRootPath,
                "src",
                "scripts",
                "flow.js");
            string fileName = _os switch
            {
                "Unix" => "node",
                _ => "node.exe"
            };
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = $"{script} {function} {_settings.SecretKey} \"{args}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            string s = string.Empty;
            string e = string.Empty;
            process.OutputDataReceived += (sender, data) => s += data.Data;
            process.ErrorDataReceived += (sender, data) => e += data.Data;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
            return s;
        }
        public SortedDictionary<string, string> Sign(SortedDictionary<string, string> ccForm)
        {
            string s = FlowJs("sign", ccForm);
            ccForm.Add("s", s);
            return ccForm;
        }
        public Customer? CustomerCreate(int id, string name, string email)
        {
            SortedDictionary<string, string> ccForm = new()
            {
                { "apiKey", _settings.ApiKey ?? string.Empty },
                { "name", name },
                { "email", email },
                { "externalId", id.ToString(CultureInfo.InvariantCulture) }
            };
            string text = FlowJs("customer/create", ccForm);
            Customer? json = JsonSerializer.Deserialize<Customer>(text, JsonCase.Camel);
            return json;
        }
        public Register? CustomerRegister(string id, Uri returnUrl)
        {
            SortedDictionary<string, string> ccForm = new()
            {
                { "apiKey", _settings.ApiKey ?? string.Empty },
                { "customerId", id },
                { "url_return", returnUrl.AbsolutePath }
            };
            string text = FlowJs("customer/register", ccForm);
            Register? json = JsonSerializer.Deserialize<Register>(text, JsonCase.Camel);
            return json;
        }
        public Customer? GetRegisterStatus(string token)
        {
            SortedDictionary<string, string> ccForm = new()
            {
                { "apiKey", _settings.ApiKey ?? string.Empty },
                { "token", token }
            };
            string text = FlowJs("customer/getRegisterStatus", ccForm);
            Customer? json = JsonSerializer.Deserialize<Customer>(text, JsonCase.Camel);
            return json;
        }
        public Customer? CustomerCharge(int id, int amount, string subject, string order)
        {
            SortedDictionary<string, string> ccForm = new()
            {
                { "apiKey", _settings.ApiKey ?? string.Empty },
                { "customerId", id.ToString(CultureInfo.InvariantCulture) },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "subject", subject },
                { "commerceOrder", order },
                { "currency", "UF" }
            };
            string text = FlowJs("customer/charge", ccForm);
            Customer? json = JsonSerializer.Deserialize<Customer>(text, JsonCase.Camel);
            return json;
        }
        public string PaymentCreate(int id, string description, int ammount, string email)
        {
            string? scheme = _httpContextAccessor.HttpContext?.Request.Scheme;
            string? confirmUri = _urlHelper.Action("Index", "Payment", null, scheme);
            string? returnUri = _urlHelper.Action("Index", "Payment", null, scheme);
            SortedDictionary<string, string> ccForm = new()
            {
                { "apiKey", _settings.ApiKey ?? string.Empty },
                { "commerceOrder", id.ToString(CultureInfo.InvariantCulture) },
                { "subject", description },
                { "currency", "CLP" },
                { "amount", ammount.ToString(CultureInfo.InvariantCulture) },
                { "email", email },
                { "urlConfirmation", confirmUri ?? string.Empty },
                { "urlReturn", returnUri ?? string.Empty }
            };
            string text = FlowJs("payment/create", ccForm);
            Register? json = JsonSerializer.Deserialize<Register>(text, JsonCase.Camel);
            return json?.Url + "?token=" + json?.Token;
        }
    }
    public class Register
    {
        public Uri? Url { get; set; }
        public string? Token { get; set; }
        public int FlowOrder { get; set; }
    }
    public class FlowSettings
    {
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
        public string? Currency { get; set; }
        public Uri? EndPoint { get; set; }
    }
}
