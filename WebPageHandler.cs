using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Reflection;

namespace AA.SeleniumHelper
{
    public class WebPageHandler
    {
        public static Dictionary<string, IWebDriver> Drivers = new();
        public static IWebDriver Driver
        {
            set
            {
                string driverName = getDriverName();
                if (Drivers.ContainsKey(driverName))
                {
                    Drivers.Remove(driverName);
                    Drivers.Add(driverName, value);
                }
                else
                {
                    Drivers.Add(driverName, value);
                }
            }
            get
            {
                string driverName = getDriverName();
                return Drivers[driverName];
            }
        }

        public static void ClickById(string id) => Driver.DoClick(By.Id(id));
        public static void ClickById(string id, bool partial)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//*[contains(@id, '{id}')]"));
            else
                ClickById(id);
        }
        public static void ClickByIdIndex(int index, string id, bool partial = false)
        {
            if (partial)
                Driver.DoClick(By.XPath($"(//*[contains(@id, '{id}')])[{index + 1}]/../.."));
            else
                Driver.DoClick(By.XPath($"(//*[@id=  '{id}'])[{index + 1}]"));
        }
        public static void ClickByIdRadio(string id) => Driver.DoClick(By.XPath($"//input[@type='radio' and @id='{id}']/.."));
        public static void ClickByName(string name) => Driver.DoClick(By.Name(name));
        public static void ClickByTitle(string name) => Driver.DoClick(By.XPath($"//*[@title='{name}']"));
        public static void ClickByText(string text, bool partial = false)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//*[contains(text(), '{text}')]"));
            else
                Driver.DoClick(By.XPath($"//*[text()='{text}']"));
        }
        public static void ClickButtonByText(string text, bool partial = false)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//button[contains(text(), '{text}')]"));
            else
                Driver.DoClick(By.XPath($"//button[text()= '{text}']"));
        }
        public static void ClickByType(string type) => Driver.DoClick(By.XPath($"//*[@type='{type}']"));
        public static void ClickByFor(string name) => Driver.DoClick(By.XPath($"//label[@for='{name}']"));
        public static void ClickByFor(string text, bool partial)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//*[contains(@for, '{text}')]"));
            else
                ClickByFor(text);
        }
        public static void ClickByClass(string text, bool partial)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//*[contains(@class, '{text}')]"));
            else
                Driver.DoClick(By.XPath($"//*[@class = '{text}']"));
        }

        public static void ClickByLabelText(string name) => Driver.DoClick(By.XPath($"//label[text()='{name}']"));
        public static void ClickByLabelText(string name, bool partial)
        {
            if (partial)
                Driver.DoClick(By.XPath($"//label[contains(text(),'{name}')]"));
            else
                ClickByLabelText(name);
        }
        public static void ClickByLabelText(string name, int index)
        {
            Driver.DoClick(By.XPath($"(//label[contains(text(),'{name}')])[{index + 1}]"));
        }

        public static string GetTextByName(string name) => Driver.WaitForElement(By.Name(name)).Text;
        public static string GetTextByFor(string name) => Driver.WaitForElement(By.XPath($"//label[@for='{name}']")).Text;
        public static string GetTextById(string id) => Driver.WaitForElement(By.Id(id)).Text;
        public static string GetTextByClass(string className) => Driver.WaitForElement(By.ClassName(className)).Text;
        public static string GetTextById(string basePath, string id) => Driver.WaitForElement(By.XPath($"{basePath}//*[@id='{id}']")).Text;
        public static void SendTextByName(string name, string value) => SendTextByXPath(By.Name(name), value);
        public static void SendTextById(string id, string value) => SendTextByXPath(By.Id(id), value);
        /// <summary>
        ///  the we have many element with the same id we need to difference it by index
        /// </summary>
        public static void SendTextById(string id, int index, string value) => SendTextByXPath(By.XPath($"(//*[@id='{id}'])[{index + 1}]"), value);
        public static void SendTextById(string id, int index, string value, bool partial)
        {
            if (partial == false)
                SendTextById(id, index, value);
            else
                SendTextByXPath(By.XPath($"(//*[contains(@id,'{id}')])[{index + 1}]"), value);
        }
        public static void SendTextByIdIndex(string id, int index, string value, bool partial)
        {
            if (partial == false)
                SendTextById(id, index, value);
            else
                SendTextByXPath(By.XPath($"(//*[contains(@id,'{id}')])[{index + 1}]"), value);
        }
        public static void SendTextById(string basePath, string id, string value) => SendTextByXPath(By.XPath($"{basePath}//*[@id= '{id}']"), value);
        public static void SendTextByIdDisplayed(string id, string value)
        {
            Driver.WaitForElement(By.Id(id));
            Driver.FindElements(By.Id(id)).First(x => x.Displayed).SendKeys(value);
        }
        public static void SendTextByIdDisplayed(string basePath, string id, string value)
        {
            Driver.FindElements(By.XPath($"{basePath}//*[@id= '{id}']")).First(x => x.Displayed).SendKeys(value);
        }

        public static void SendTextByXPath(By by, string value)
        {
            Driver.DoClick(by);
            IWebElement webElement = Driver.WaitForClickable(by);
            if (!string.IsNullOrEmpty(webElement.GetAttribute("value")))
                webElement.SendKeys(Keys.Control + "A");
            webElement.SendKeys(value);
        }

        private static string getDriverName()
        {
            if (TestContext.CurrentContext.Test.MethodName == null)
            {
                return TestContext.CurrentContext.Test.FullName;
            }
            else
            {
                return TestContext.CurrentContext.Test.ClassName ?? throw new Exception("no name found");
            }
        }

        public static void ClickCheckBoxById(string id, int index)
        {
            Driver.DoClick(By.XPath($"(//input[@id='{id}' and @type='checkbox']//..)[{index + 1}]"));
        }
        public static void ClickCheckBoxById(string id)
        {
            Driver.DoClick(By.XPath($"//input[@id='{id}' and @type='checkbox']//.."));
        }
        public static void SelectById<T>(string id, T value) where T : struct, Enum
        {
            int listIndex = Array.IndexOf(Enum.GetValues(value.GetType()), value) + 1;
            string enumName = Enum.GetName(value) ?? throw new Exception("no valid enum");
            var selectElement = Driver.WaitForClickable(By.Id(id));
            var select = new SelectElement(selectElement);
            select.SelectByIndex(listIndex);
        }

        public static void ChooseFile(By by, string FileName)
        {
            string fullFilePath;
            if (Extensions.Remote == true)
            {
                fullFilePath = "/home/Files/" + FileName;
            }
            else
            {
                fullFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Files\\" + FileName;
            }
            var updateFile = Driver.FindElements(by).ToList();
            updateFile[0].SendKeys(fullFilePath);
        }
    }
}
