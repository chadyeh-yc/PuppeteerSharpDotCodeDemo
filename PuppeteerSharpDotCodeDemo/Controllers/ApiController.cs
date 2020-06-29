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
        private readonly DirectoryInfo _snapshotInfo;

        public ApiController(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            var localChromiumInfo = new DirectoryInfo(Path.Combine(_hostingEnvironment.ContentRootPath, ".local-chromium"));
            if (!localChromiumInfo.Exists)
            {
                localChromiumInfo.Create();
            }

            _snapshotInfo = new DirectoryInfo(Path.Combine(_hostingEnvironment.ContentRootPath, "Snapshot"));
            if (!_snapshotInfo.Exists)
            {
                _snapshotInfo.Create();
            }
            _browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = localChromiumInfo.FullName
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

        [HttpGet]
        [Route("ScreenShot")]
        public async Task<IActionResult> TestScreenShotData()
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision),
                Headless = true
            });
            var page = await browser.NewPageAsync();
            // 開啟 udemy 網頁
            await page.GoToAsync("https://www.udemy.com/");
            // 判斷是否載入完成
            await page.WaitForSelectorAsync("div.browse-container");
            // 截圖
            var screenshotsDataAsync = await page.ScreenshotDataAsync(new ScreenshotOptions() { FullPage = true });
            return File(screenshotsDataAsync, "image/jpeg", $"udemy{DateTime.Now:yyyyMMddHHmmssffff}PuppeteerTest.jpg");
        }

        [HttpGet]
        [Route("ScreenShotFreeway")]
        public async Task<IActionResult> TestScreenShotFreeway()
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision),
                Headless = true
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://1968.freeway.gov.tw/");
            //透過SetViewport控制視窗大小決定抓圖尺寸
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1024,
                Height = 768
            });
            foreach (var region in new[] { "N", "C", "P" })
            {
                // 呼叫網頁程式方法切換區域
                await page.EvaluateExpressionAsync($"region('{region}')");
                // 要等待網頁切換顯示完成再抓圖
                await page.WaitForSelectorAsync("div.fwoverlay");
                // 抓網頁畫面存檔
                await page.ScreenshotAsync(Path.Combine(_snapshotInfo.FullName, $@"{ DateTime.Now:yyyyMMddHHmmss}FreewayTraffic{region}.png"));
            }
            return Ok();
        }

        [HttpGet]
        [Route("ScreenShotFreewayConnect")]
        public async Task<IActionResult> TestScreenShotFreewayConnect()
        {
            var browser = await Puppeteer.ConnectAsync(new ConnectOptions
            {
                BrowserWSEndpoint = "ws://localhost:3000",
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://1968.freeway.gov.tw/");
            //透過SetViewport控制視窗大小決定抓圖尺寸
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1024,
                Height = 768
            });
            foreach (var region in new[] { "N", "C", "P" })
            {
                // 呼叫網頁程式方法切換區域
                await page.EvaluateExpressionAsync($"region('{region}')");
                // 要等待網頁切換顯示完成再抓圖
                await page.WaitForSelectorAsync("div.fwoverlay");
                // 抓網頁畫面存檔
                await page.ScreenshotAsync(Path.Combine(_snapshotInfo.FullName, $@"{ DateTime.Now:yyyyMMddHHmmss}FreewayTraffic{region}.png"));
            }
            return Ok();
        }
    }
}
