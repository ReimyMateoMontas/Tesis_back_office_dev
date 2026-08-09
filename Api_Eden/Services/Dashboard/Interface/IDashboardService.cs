using Api_Eden.Services.DashboardService;

namespace Api_Eden.Services.Dashboard.Interface
{
   
        public interface IDashboardService
        {
            Task<DashboardResumenDto> GetResumenAsync();
        }
    
}
