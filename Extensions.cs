using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace AA.SeleniumHelper
{
    static public class Extensions
    {
        static public IWebElement WaitForElement(this IWebDriver driver, By by)
        {
            CancellationToken token = new CancellationToken();
            return driver.WaitForElement(by, TimeSpan.FromSeconds(5), token);
        }
        static public IWebElement WaitForElement(this IWebDriver driver, By by, CancellationToken token)
        {
            return driver.WaitForElement(by, TimeSpan.FromSeconds(5), token);
        }
        static public IWebElement WaitForElement(this IWebDriver driver, By by, TimeSpan timeOut)
        {
            CancellationToken token = new CancellationToken();
            return driver.WaitForElement(by, timeOut, token);

        }
        static public IWebElement WaitForElement(this IWebDriver driver, By by, TimeSpan timeOut, CancellationToken token)
        {

            WebDriverWait wait = new WebDriverWait(driver, timeOut);
            try
            {
                return wait.Until(ExpectedConditions.ElementExists(by), token);
            }
            catch (Exception ex) when (ex is StaleElementReferenceException)
            {
                Thread.Sleep(100);
                return wait.Until(ExpectedConditions.ElementExists(by), token);
            }
            catch (Exception ex)
            {
                if (ExtraErrorHandler(driver, ex))
                    return wait.Until(ExpectedConditions.ElementExists(by), token);
                else
                    throw ex;
            }
        }

        static public IWebElement WaitForClickable(this IWebDriver driver, By by)
        {
            return driver.WaitForClickable(by, TimeSpan.FromSeconds(5));
        }
        static public IWebElement WaitForClickable(this IWebDriver driver, By by, TimeSpan timeOut)
        {
            var wait = new WebDriverWait(driver, timeOut);
            long start = DateTimeOffset.Now.ToUnixTimeSeconds();
            IWebElement webElement = driver.WaitForElement(by);
            try
            {
                return wait.Until(ExpectedConditions.ElementToBeClickable(by));
            }
            catch (Exception)
            {
                var actions = new Actions(driver);
                actions.MoveToElement(webElement);
                actions.Perform();
                return wait.Until(ExpectedConditions.ElementToBeClickable(by));
            }
        }
        public static Func<IWebDriver, Exception, bool> ExtraErrorHandler = (driver, ex) => true;

        public static bool Remote { get; set; }

        static public void DoClick(this IWebDriver driver, By by, int seconds = 5)
        {
            Exception lastException = new Exception("too many try" + by.ToString());
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    long start = DateTimeOffset.Now.ToUnixTimeSeconds();
                    IWebElement webElement = wait.Until(ExpectedConditions.ElementExists(by));
                    if (webElement.Displayed && webElement.Enabled)
                    {
                        webElement.Click();
                    }
                    else
                    {
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(webElement).Perform();
                        wait.Until(ExpectedConditions.ElementToBeClickable(by)).Click();
                    }
                    long end = DateTimeOffset.Now.ToUnixTimeSeconds();
                    return;
                }
                catch (Exception ex) when (ex is InvalidSelectorException)
                {
                    Console.WriteLine($"InvalidSelectorException {by.ToString()}");
                    throw;
                }
                catch (Exception ex) when (ex is StaleElementReferenceException)
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"StaleElementReferenceException by {by.ToString()}");
                    lastException = ex;
                    continue;
                }

                catch (Exception ex)
                {
                    if (ExtraErrorHandler(driver, ex))
                        continue;
                }
            }
            Console.WriteLine("unrecoverable error when finding an element ");
            throw lastException;
        }
        static public void ClickAndDisappears(this IWebDriver driver, By by, bool tryClickingManytime = false)
        {
            driver.DoClick(by);
            try
            {
                driver.WaitForElementToBeGone(by, TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
                if (tryClickingManytime)
                {
                    driver.DoClick(by);
                    driver.WaitForElementToBeGone(by, TimeSpan.FromSeconds(5));
                }
                else throw;
            }
        }
        static public IEnumerable<IWebElement> DoFindElements(this IWebDriver driver, By by)
        {
            try
            {
                Thread.Sleep(500); //FindElements is a time problematic call 
                return driver.FindElements(by);
            }
            catch (Exception ex) when (ex is StaleElementReferenceException)
            {
                Thread.Sleep(500);  //FindElements is a time problematic call 
                return driver.FindElements(by);
            }
        }

        static public void ClickAndDisappears(this IWebElement webElement)
        {
            webElement.ClickAndDisappears(TimeSpan.FromSeconds(5), 10);
        }
        static public void ClickAndDisappears(this IWebElement webElement, TimeSpan timeOut, int tried)
        {
            int i = 0;
            try
            {
                do
                {
                    i++;
                    webElement.Click();
                    Thread.Sleep(timeOut / tried);
                }
                while (!webElement.Displayed && i < tried);
            }
            catch (Exception ex)
                when (ex is NoSuchElementException ||
                      ex is ElementNotVisibleException ||
                      ex is StaleElementReferenceException)
            {
                return;
            }
            catch (InvalidSelectorException ise)
            {
                throw ise;
            }
            for (int j = 1; j < tried; j++)
            {
                try
                {
                    ExpectedConditions.StalenessOf(webElement);

                    return;
                }
                catch (Exception)
                {
                }
            }
        }
        static public void WaitForElementToBeGone(this IWebDriver driver, By by, TimeSpan timeOut)
        {
            WebDriverWait wait = new WebDriverWait(driver, timeOut);
            try
            {
                driver.FindElement(by);

            }
            catch (Exception)
            {
                return;
                // no loading element so continue 
            }
            try
            {
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(by));
            }
            catch (Exception e) when (e is NoSuchElementException || e is WebDriverException)
            {
                return;
            }
            catch (WebDriverTimeoutException wdte)
            {
                throw new Exception($"Timeout for wait for Staleness Of {by} ", wdte);
            }

        }
        public static void ScreenShot(this ITakesScreenshot driver, string FileName)
        {
            var screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshot");
            Directory.CreateDirectory(screenshotPath);
            Screenshot screenshot = driver.GetScreenshot();
            string screenshotFile = Path.Combine(screenshotPath, FileName + ".png");
            Console.WriteLine(screenshotFile);
            screenshot.SaveAsFile(screenshotFile, ScreenshotImageFormat.Png);
            TestContext.AddTestAttachment(screenshotFile, "Screenshot");
        }
        public static void ScreenShot(this ITakesScreenshot driver, TestContext testContext)
        {
            string failString = "";
            var screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshot");
            Directory.CreateDirectory(screenshotPath);
            Screenshot screenshot = driver.GetScreenshot();
            if (testContext.Result.Outcome.Status.Equals(TestStatus.Failed))
            {
                failString = "fail";
            }
            string screenshotFile = Path.Combine(screenshotPath, failString + TestContext.CurrentContext.Test.FullName + DateTime.Now.ToString("T").Replace(":", "_") + ".png").Replace("(", "_").Replace(")", "_").Replace("\"", "_").Replace("_", "_");
            Console.WriteLine(screenshotFile);
            screenshot.SaveAsFile(screenshotFile, ScreenshotImageFormat.Png);
            TestContext.AddTestAttachment(screenshotFile, "Screenshot");
        }
        public static void ScreenShot(this ITakesScreenshot driver)
        {
            var screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshot");
            Directory.CreateDirectory(screenshotPath);
            Screenshot screenshot = driver.GetScreenshot();
            string screenshotFile = Path.Combine(screenshotPath, TestContext.CurrentContext.Test.FullName + DateTime.Now.ToString("T").Replace(":", "_") + ".png").Replace("(", "_").Replace(")", "_").Replace("\"", "_").Replace("_", "_");
            Console.WriteLine(screenshotFile);
            screenshot.SaveAsFile(screenshotFile, ScreenshotImageFormat.Png);
            TestContext.AddTestAttachment(screenshotFile, "Screenshot");
        }
        static public IWebElement WaitForElementAnimationToFinish(this IWebDriver driver, By by, TimeSpan? timeOut = null)
        {
            WebDriverWait wait = new WebDriverWait(driver, timeOut ?? TimeSpan.FromSeconds(30));
            return wait.Until(WaitForElementAnimationToFinishFunc(by));
        }

        private static Func<IWebDriver, IWebElement> WaitForElementAnimationToFinishFunc(By by)
        {
            double x = 0;
            double y = 0;
            double width = 0;
            double height = 0;

            return delegate (IWebDriver driver)
            {
                try
                {
                    IWebElement elem = driver.FindElement(by);
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    dynamic rect = js.ExecuteScript("var rect = arguments[0].getBoundingClientRect(); return { x: rect.x, y: rect.y, width: rect.width, height: rect.height };", elem);

                    double newX = (double)rect["x"];
                    double newY = (double)rect["y"];
                    double newWidth = (double)rect["width"];
                    double newHeight = (double)rect["height"];

                    if (newX != x || newY != y || newWidth != width || newHeight != height)
                    {
                        x = newX;
                        y = newY;
                        width = newWidth;
                        height = newHeight;
                        return null;
                    }

                    return elem;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            };
        }
        static public string Text(this Func<IWebElement> IWebElementFunc)
        {
            try
            {
                return IWebElementFunc().Text;
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(100);
                return IWebElementFunc().Text;
            }
        }
        static public void Clear(this Func<IWebElement> IWebElementFunc)
        {
            try
            {
                IWebElementFunc().Clear();
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(100);
                IWebElementFunc().Clear();
            }

        }
        static public void SendKeys(this Func<IWebElement> IWebElementFunc, string test)
        {
            try
            {
                IWebElementFunc().SendKeys(test);
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(100);
                IWebElementFunc().SendKeys(test);
            }
        }
    }
}
