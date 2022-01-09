﻿using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Environmental.Plancton;
using BiblioMit.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BiblioMit.Services
{
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
            var file = Path.Combine(_environment.ContentRootPath, "html", "PullRecords.html");
            using var streamWriter = new StreamWriter(file);
            await streamWriter.WriteLineAsync("FECHA: " + DateTime.Now.ToShortDateString()).ConfigureAwait(false);
            await streamWriter.WriteLineAsync("<br />").ConfigureAwait(false);
            streamWriter.Close();

            var dateQueryFormat = "dd/MM/yyyy";
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
            foreach (PlanktonUser user in await _context.PlanktonUsers.ToListAsync(stoppingToken).ConfigureAwait(false))
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
                var signinResponse = await client.PostAsync(login, credentials, stoppingToken).ConfigureAwait(false);
                if (signinResponse.Headers.Location.OriginalString == "clipal_inicio.asp")
                {
                    //bool hasAssays = _context.PlanktonAssays.Any(p => p.PlanktonUserId == user.Id);
                    var month = 
                    //    hasAssays ?
                    //_context.PlanktonAssays.Where(p => p.PlanktonUserId == user.Id).Max(p => p.SamplingDate) :
                    //min date plankton 
                    new DateTime(2003, 1, 1);
                    while (month <= DateTime.Now)
                    {
                        var searcher = new Uri("http://sispal.plancton.cl/clientes/psmb_buscamuestra.asp");
                        var from = month.ToString(dateQueryFormat, CultureInfo.CurrentCulture);
                        var to = month.GetLastDayOfMonth().ToString(dateQueryFormat, CultureInfo.CurrentCulture);
                        using var query = new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                { "D1", "todo" },
                                { "T1", "" },
                                { "T2", from },
                                { "T3", to },
                                { "B1", "Buscar" }
                            });
                        var response = await client.PostAsync(searcher, query, stoppingToken).ConfigureAwait(false);
                        var html = await response.Content.ReadAsStringAsync(stoppingToken).ConfigureAwait(false);
                        var regex = new Regex(@"psmb_informe_ue_xls\.asp\?codigo\=([0-9]+)");
                        foreach (Match match in regex.Matches(html))
                        {
                            var fma = int.Parse(match.Groups[1].Value, CultureInfo.CurrentCulture);
                            var record = await _context.PlanktonAssays.FirstOrDefaultAsync(p => p.Id == fma, stoppingToken).ConfigureAwait(false);
                            if (record == null)
                            {
                                var assayurl = new Uri($"http://sispal.plancton.cl/clientes/psmb_informe_uesf.asp?codigo={fma}");
                                var assayhtml = await client.GetAsync(assayurl, stoppingToken).ConfigureAwait(false);
                                try
                                {
                                    var import = await _import.AddAsync(await assayhtml.Content.ReadAsStreamAsync(stoppingToken).ConfigureAwait(false)).ConfigureAwait(false);
                                    if (import.IsCompletedSuccessfully)
                                    {
                                        Console.WriteLine($"Added Plankton Assay FMA:{fma}");
                                    }
                                }
                                catch (FormatException ex)
                                {
                                    if(!ex.Message.Contains("El archivo ingresado no contiene registros ni tablas", StringComparison.Ordinal))
                                    {
                                        using var sw = new StreamWriter(file, true);
                                        await sw.WriteLineAsync(HttpUtility.HtmlEncode($"Usuario Plancton: {user.Name}")).ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        await sw.WriteLineAsync(HttpUtility.HtmlEncode($"FMA:{fma}")).ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        await sw.WriteLineAsync(HttpUtility.HtmlEncode($"Mes/Año Muestreo:{month.ToString("MM/yyyy", CultureInfo.CurrentCulture)}")).ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        await sw.WriteLineAsync("ERROR:").ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        await sw.WriteLineAsync(HttpUtility.HtmlEncode(ex.Message)).ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        await sw.WriteLineAsync(ex.StackTrace).ConfigureAwait(false);
                                        await sw.WriteLineAsync("<br />").ConfigureAwait(false);
                                        sw.Close();
                                    }
                                }
                            }
                            else
                            {
                                if(record.PlanktonUserId != user.Id)
                                {
                                    record.PlanktonUserId = user.Id;
                                    _context.PlanktonAssays.Update(record);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        month = new DateTime(month.Year, month.Month, 1).AddMonths(1);
                    }
                }
                else
                {
                    LogCouldnotLogIn(_logger, user.Name);
                }
            }
            return;
        }
        [LoggerMessage(0, LogLevel.Error, "User {UserName} could not login, Check password.")]
        static partial void LogCouldnotLogIn(ILogger logger, string UserName);
    }
}
