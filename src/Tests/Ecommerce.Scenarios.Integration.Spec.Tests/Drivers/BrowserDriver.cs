using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow.Infrastructure;

namespace Ecommerce.Scenarios.Integration.Spec.Tests.Drivers
{
    public class BrowserDriver : IDisposable
    {
        private readonly ISpecFlowOutputHelper _specFlowOutputHelper;
        private readonly Lazy<IWebDriver> _currentWebDriverLazy;
        private bool _isDisposed;

        /// <summary>
        /// The Selenium IWebDriver instance
        /// </summary>
        public IWebDriver Current => _currentWebDriverLazy.Value;

        public BrowserDriver(ISpecFlowOutputHelper specFlowOutputHelper)
        {
            _specFlowOutputHelper = specFlowOutputHelper;
            _currentWebDriverLazy = new Lazy<IWebDriver>(CreateWebDriver);
        }
        
        /// <summary>
        /// Creates the Selenium web driver (opens a browser)
        /// </summary>
        /// <returns></returns>
        private IWebDriver CreateWebDriver()
        {
            //We use the Chrome browser
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var chromeOptions = new ChromeOptions();
            var chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
            _specFlowOutputHelper.WriteLine("Browser launched");
            return chromeDriver;
        }
        
        /// <summary>
        /// Disposes the Selenium web driver (closing the browser)
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_currentWebDriverLazy.IsValueCreated)
            {
                Current.Quit();
                _specFlowOutputHelper.WriteLine("Browser closed");
            }

            _isDisposed = true;
        }
    }
}