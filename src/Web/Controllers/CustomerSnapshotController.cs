using CustomerSnapshot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSnapshot.Web.Controllers;

public class CustomerSnapshotController : Controller
{
    private readonly ICustomerSnapshotService _customerSnapshotService;

    public CustomerSnapshotController(ICustomerSnapshotService customerSnapshotService)
    {
        _customerSnapshotService = customerSnapshotService;
    }

    [HttpGet("/CustomerSnapshot/{customerId:int}")]
    public async Task<IActionResult> Index(int customerId, CancellationToken cancellationToken)
    {
        var viewModel = await _customerSnapshotService.GetSnapshotAsync(customerId, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }
}
