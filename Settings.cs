namespace AA.SeleniumHelper
{
    public class Settings
    {
        public string? URL { get; set; }

        public bool? remote { get; set; }

        public DriverNameEnum DriverName { get; set; }
    }
    public enum DriverNameEnum { Chrome, FireFox, Edge, Phantom }
}
