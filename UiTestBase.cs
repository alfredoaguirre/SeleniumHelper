using AA.SeleniumHelper;
using OpenQA.Selenium;


namespace AA.SeleniumHelper
{
    public class UiTestBase
    {
        public static Dictionary<string, Settings> settingsSet = new Dictionary<string, Settings>();
        public static Settings Settings
        {
            set
            {
                string driverName = GetSettingName();
                Console.WriteLine("staring new test class: " + driverName);
                if (settingsSet.ContainsKey(driverName))
                {
                    settingsSet.Remove(driverName);
                    settingsSet.Add(driverName, value);
                }
                else
                {
                    settingsSet.Add(driverName, value);
                }
            }
            get
            {
                string driverName = GetSettingName();
                return settingsSet[driverName];
            }
        }

        private static string GetSettingName()
        {
            if (TestContext.CurrentContext.Test.MethodName == null)
            {
                return TestContext.CurrentContext.Test.FullName;
            }
            else
            {
                return TestContext.CurrentContext.Test.ClassName ?? throw new Exception("no driver found");
            }
        }

        [TearDown]
        public void AfterTest()
        {
            ((ITakesScreenshot)WebPageHandler.Driver).ScreenShot(TestContext.CurrentContext);

            Console.WriteLine(WebPageHandler.Driver.Url);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            WebPageHandler.Driver.Quit();
        }
    }
}
