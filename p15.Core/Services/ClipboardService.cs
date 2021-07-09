namespace p15.Core.Services
{
    public class ClipboardService
    {
        private readonly PowershellService _powershellService;

        public ClipboardService(PowershellService powershellService)
        {
            _powershellService = powershellService;
        }

        public void CopyToCliboard(string text)
        {
            _powershellService.Execute(ps =>
            {
                ps.AddCommand("Set-Clipboard");
                ps.AddParameter("Value", text);
            });
        }
    }
}
