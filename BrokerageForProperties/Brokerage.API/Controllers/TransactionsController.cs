using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.API.Controllers;


// Provides authenticated Admin endpoints for retrieving properties whose status = "completed"
// property transactions with server-side filtering and pagination.
[ApiController]
[Route("api/transactions")]
[Authorize(Roles = "Admin")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(
        TransactionService transactionService)
    {
        _transactionService =
            transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] TransactionFilterRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate.HasValue &&
            request.EndDate.HasValue &&
            request.StartDate >
            request.EndDate)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message =
                        "Start date cannot be later than end date."
                });
        }

        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice >
            request.MaxPrice)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message =
                        "Minimum price cannot be greater than maximum price."
                });
        }

        TransactionListResponse response =
            await _transactionService
                .GetCompletedTransactionsAsync(
                    request,
                    cancellationToken);

        return Ok(response);
    }
}