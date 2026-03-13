using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Policy = "ManagerOrAdmin")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    /// <summary>
    /// List all customers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> ListCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? term = null)
    {
        return Ok(await customerService.ListCustomersAsync(page, size, term));
    }

    /// <summary>
    /// Get customer details by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(string id)
    {
        var customer = await customerService.GetCustomerByIdAsync(id);
        if (customer is null)
            return NotFound(new { error = "Customer not found." });

        return Ok(customer);
    }

    /// <summary>
    /// Retrieve customer ID image. Admin only.
    /// </summary>
    [HttpGet("{id}/id-image")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetCustomerIdImage(string id)
    {
        var file = await customerService.GetCustomerIdImageAsync(id);
        if (file is null)
            return NotFound(new { error = "Customer ID image not found." });

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }
}
