using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:int}/dashboard")]
    [Authorize]
    public sealed class CustomerDashboardController : ControllerBase
    {
        private readonly ICustomerDashboardService _dashboard;

        public CustomerDashboardController(ICustomerDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerDashboardDto>> Get(int customerId, CancellationToken ct)
            => Ok(await _dashboard.GetAsync(customerId, ct));
    }
}
