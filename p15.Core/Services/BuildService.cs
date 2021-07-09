using System.Threading.Tasks;

namespace p15.Core.Services
{
    public class BuildService
    {
        private readonly PowershellService _powershellService;

        public BuildService(PowershellService powershellService)
        {
            _powershellService = powershellService;
        }

        public async Task Build(string path)
        {
            await _powershellService.ExecuteAsync(ps =>
            {
                ps.AddCommand("Build-Application");
                ps.AddParameter("Path", path);
            });
        }
    }
}
