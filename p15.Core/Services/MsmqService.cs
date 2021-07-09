using System.Threading.Tasks;

namespace p15.Core.Services
{
    public class MsmqService
    {
        private readonly PowershellService _powershellService;

        public MsmqService(PowershellService powershellService)
        {
            _powershellService = powershellService;
        }

        public async Task ConfigureQueues(string path)
        {
            await _powershellService.ExecuteAsync(ps =>
            {
                ps.AddCommand("Initialize-MissingMessageQueues");
                ps.AddParameter("Path", path);
            });
        }
    }
}
