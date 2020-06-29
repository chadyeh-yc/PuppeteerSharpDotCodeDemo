using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace PuppeteerSharpDotCodeDemo.Controllers
{

    public class ApiController : Controller
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly BrowserFetcher _browserFetcher;

        public ApiController(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = Path.Combine(_hostingEnvironment.ContentRootPath, ".local-chromium")
            });
            _browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
        }

        [HttpGet]
        [Route("Pdf")]
        public async Task<IActionResult> TestPdfData()
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision),
                Headless = true
            });
            var page = await browser.NewPageAsync();
            // 開啟 Google 網頁
            await page.GoToAsync("https://www.google.com.tw/");
            // 輸入搜尋Keyword
            await page.TypeAsync("input.gLFyf.gsfi", "puppeteer sharp");
            // 按下搜尋按鈕
            await page.ClickAsync("input.gNO89b");
            // 確認出現搜尋結果
            await page.WaitForSelectorAsync("#rcnt");
            // 輸出
            const string headerTemplate = @"<div style=""font-size: 30px; width: 200px; height: 200px; background-color: black; color: white; margin: 20px;"">Header 1</div>";
            const string footerTemplate = @"<div style=""font-size: 30px; width: 50px; height: 50px; background-color: red; color:black; margin: 20px;"">Footer</div>";
            var pdfDataAsync = await page.PdfDataAsync(new PdfOptions
            {
                DisplayHeaderFooter = true,
                HeaderTemplate = headerTemplate,
                FooterTemplate = footerTemplate,
                Format = PaperFormat.A4
            });
            return File(pdfDataAsync, "application/pdf", $"{DateTime.Now:yyyyMMddHHmmssffff}PuppeteerTest.pdf");
        }
    }
}
