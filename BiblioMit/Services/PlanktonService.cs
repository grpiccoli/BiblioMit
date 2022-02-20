using BiblioMit.Data;
//using BiblioMit.Extensions;
//using BiblioMit.Models;
//using BiblioMit.Models.Entities.Environmental.Plancton;
using BiblioMit.Services.Interfaces;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
using System.Globalization;
//using System.IO;
using System.Net;
//using System.Net.Http;
using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
using System.Web;

namespace BiblioMit.Services
{
    public class UserQueryableModel
    {
        private readonly DateTime _mindate = new(2003,1,1);
        public UserQueryableModel(string name, string password, DateTime? maxDate)
        {
            Name = name;
            Password = password;
            MaxDate = maxDate ?? _mindate;
        }
        public string Name { get; }
        public string Password { get; }
        public DateTime MaxDate { get; set; }
    }
    public partial class PlanktonService : IPlanktonService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IImport _import;
        private readonly IWebHostEnvironment _environment;
        public PlanktonService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IImport import,
            ILogger<PlanktonService> logger
            )
        {
            _import = import;
            _environment = environment;
            _context = context;
            _logger = logger;
        }
        public async Task PullRecordsAsync(CancellationToken stoppingToken)
        {
            string file = Path.Combine(_environment.ContentRootPath, "StaticFiles", "html", "PullRecords.html");
            using StreamWriter sw = new(file);
            await sw.WriteLineAsync("<hr />").ConfigureAwait(false);
            await sw.WriteLineAsync("FECHA: " + DateTime.Now.ToShortDateString()).ConfigureAwait(false);
            await sw.WriteLineAsync("<br />").ConfigureAwait(false);
            await sw.WriteLineAsync("<hr />").ConfigureAwait(false);

            string dateQueryFormat = "dd/MM/yyyy";
            CookieContainer cookieJar = new();
            using HttpClientHandler handler = new()
            {
                UseCookies = true,
                UseDefaultCredentials = false,
                CookieContainer = cookieJar,
                AllowAutoRedirect = false,
                CheckCertificateRevocationList = true
            };
            using HttpClient client = new(handler);
            List<UserQueryableModel> users = _context.PlanktonUsers
                .Where(p => p.Name != null && p.Password != null)
                .Select(u => new UserQueryableModel(u.Name ?? string.Empty, u.Password ?? string.Empty, u.Assays.Max(a => a.SamplingDate))).ToList();
            HashSet<string> assayIds = new(_context.PlanktonAssays.Select(p => p.Id.ToString()).ToList(), null);
            foreach (UserQueryableModel user in users)
            {
                Uri login = new("http://sispal.plancton.cl/clientes/clipal_validausuario.asp");
                using FormUrlEncodedContent credentials =
                    new(
                        new Dictionary<string, string>
                        {
                            { "usuario", user.Name },
                            { "passwd", user.Password },
                            { "B1", "Entrar" }
                        });
                HttpResponseMessage signinResponse = await client.PostAsync(login, credentials, stoppingToken).ConfigureAwait(false);
                if (signinResponse.Headers.Location?.OriginalString == "clipal_inicio.asp")
                {
                    //while (user.MaxDate <= DateTime.Now)
                    //{
                    Uri searcher = new("http://sispal.plancton.cl/clientes/psmb_buscamuestra.asp");
                    string from = user.MaxDate.ToString(dateQueryFormat, CultureInfo.CurrentCulture);
                    //string to = user.MaxDate.GetLastDayOfMonth().ToString(dateQueryFormat, CultureInfo.CurrentCulture);
                    string to = DateTime.Now.ToString(dateQueryFormat, CultureInfo.CurrentCulture);
                    using FormUrlEncodedContent query = new(
                            new Dictionary<string, string>
                            {
                                { "D1", "todo" },
                                { "T1", "" },
                                { "T2", from },
                                { "T3", to },
                                { "B1", "Buscar" }
                            });
                    HttpResponseMessage response = await client.PostAsync(searcher, query, stoppingToken).ConfigureAwait(false);
                    string html = await response.Content.ReadAsStringAsync(stoppingToken).ConfigureAwait(false);
                    Regex regex = new(@"psmb_informe_ue_xls\.asp\?codigo\=([0-9]+)");

                    foreach (Match match in regex.Matches(html))
                    {
                        string fma = match.Groups[1].Value;
                        bool inDB = assayIds.Contains(fma);
                        if (!inDB)
                        {
                            Uri assayurl = new($"http://sispal.plancton.cl/clientes/psmb_informe_uesf.asp?codigo={fma}");
                            HttpResponseMessage assayhtml = await client.GetAsync(assayurl, stoppingToken).ConfigureAwait(false);
                            try
                            {
                                Task import = await _import.AddAsync(await assayhtml.Content.ReadAsStreamAsync(stoppingToken).ConfigureAwait(false)).ConfigureAwait(false);
                                if (import.IsCompletedSuccessfully)
                                {
                                    Console.WriteLine($"Added Plankton Assay FMA:{fma}");
                                }
                            }
                            catch (FormatException ex)
                            {
                                if (!ex.Message.Contains("El archivo ingresado no contiene registros ni tablas", StringComparison.Ordinal))
                                {
                                    await sw.WriteLineAsync(HttpUtility.HtmlEncode($"Usuario Plancton: {user.Name}")).ConfigureAwait(false);
                                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    await sw.WriteLineAsync(HttpUtility.HtmlEncode($"FMA:{fma}")).ConfigureAwait(false);
                                    //await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    //await sw.WriteLineAsync(HttpUtility.HtmlEncode($"Mes/Año Muestreo:{user.MaxDate.ToString("MM/yyyy", CultureInfo.CurrentCulture)}")).ConfigureAwait(false);
                                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    await sw.WriteLineAsync("ERROR:").ConfigureAwait(false);
                                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    await sw.WriteLineAsync(HttpUtility.HtmlEncode(ex.Message)).ConfigureAwait(false);
                                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    await sw.WriteLineAsync(ex.StackTrace).ConfigureAwait(false);
                                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                    await sw.WriteLineAsync("<hr />").ConfigureAwait(false);
                                }
                            }
                        }
                        //else if (.PlanktonUserId != user.Id)
                        //{
                        //    record.PlanktonUserId = user.Id;
                        //    _context.PlanktonAssays.Update(record);
                        //    _context.SaveChanges();
                        //}
                    }
                    //user.MaxDate = user.MaxDate.AddMonths(1);
                    //}
                }
                else
                {
                    await sw.WriteLineAsync(HttpUtility.HtmlEncode($"Usuario Plancton: {user.Name}"));
                    await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                    await sw.WriteLineAsync(HttpUtility.HtmlEncode($"ERROR: Inicio de sesión inválido, verificar credenciales"));
                    await sw.WriteLineAsync("<hr />").ConfigureAwait(false);
                    LogCouldnotLogIn(_logger, user.Name);
                }
            }
            sw.Close();
            return;
        }
        [LoggerMessage(0, LogLevel.Error, "User {UserName} could not login, Check password.")]
        static partial void LogCouldnotLogIn(ILogger logger, string UserName);
    }
}
