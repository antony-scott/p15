using System.Threading.Tasks;
using p15.Core.Models;

namespace p15.Core.Services
{
    public class FirewallService
    {
        private readonly PowershellService _powershellService;

        public FirewallService(PowershellService powershellService)
        {
            _powershellService = powershellService;
        }

        public async Task AddRule(FirewallRule rule)
        {
            await _powershellService.ExecuteAsync(ps =>
            {
                ps.AddCommand("Open-Firewall");
                ps.AddParameter("Description", $"{rule.PackageName} ({rule.Name})");
                ps.AddParameter("Port", rule.Port);
            });
        }
    }
}
