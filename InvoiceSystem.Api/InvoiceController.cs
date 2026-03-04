using System.Linq;
using InvoiceSystem.Api.Dtos;
using InvoiceSystem.Application.Interfaces;
using InvoiceSystem.Application.Models;
using InvoiceSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvoiceSystem.Api
{
	[ApiController]
	[Route("invoices")]
	public class InvoiceController : ControllerBase
	{
		private readonly IInvoiceService _service;
		private readonly ILogger<InvoiceController> _logger;

		public InvoiceController(IInvoiceService service,ILogger<InvoiceController> logger)
		{
			_service = service;
			 _logger = logger;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateInvoice(CreateInvoiceRequest request)
		{
			_logger.LogInformation("Creating invoice");
			var dueDateUtc = request.DueDate
			.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
			var id = await _service.CreateInvoiceAsync(request.Amount, dueDateUtc);

			return Created(string.Empty, new { id });
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetInvoices()
		{
			_logger.LogInformation("Fetching all invoices");
			var invoices = (await _service.GetAllInvoicesAsync())
				.Select(i => new InvoiceResponse(
					i.Id,
					i.Amount.ToString("F2"),
					i.PaidAmount.ToString("F2"),
					i.DueDate,
					i.DisplayStatus.ToString()
				));

			return Ok(invoices);
		}

		[HttpPost("{id}/payments")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> AddPayment(long id, PaymentRequest request)
		{
			_logger.LogInformation("Adding payment");
			await _service.AddPaymentAsync(id, request.Amount);
			return Ok();
		}

		[HttpPost("process-overdue")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ProcessOverdue(ProcessOverdueRequest request)
		{
			_logger.LogInformation("Processing Overdue");
			await _service.ProcessOverdueAsync(request.LateFee, request.OverdueDays);
			return Ok();
		}

		[HttpPost("{id}/cancel")]
		[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Cancel(long id)
		{
			_logger.LogInformation("Cancelling invoice");
			await _service.CancelInvoiceAsync(id);
			return Ok();
		}

		//[HttpGet("pending")]
		//public IActionResult GetPending()
		//{
		//	var result = _service.GetPaymentPendingInvoices();
		//	return Ok(result);
		//}

		//[HttpGet("paid")]
		//public IActionResult GetPaid()
		//{
		//	var result = _service.GetPaidInvoices();
		//	return Ok(result);
		//}

		[HttpGet("search")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Search(
			[FromQuery] string? status,
			[FromQuery] DateOnly? fromDate,
			[FromQuery] DateOnly? toDate,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			_logger.LogInformation("Searching via SearchFilter");
			status = status?.ToLowerInvariant();
			var filter = new InvoiceSearchFilter
			{
				Status = status,
				FromDate = fromDate,
				ToDate = toDate
			};

			var result = await _service.SearchInvoicesAsync(filter, page, pageSize);
			return Ok(result);
		}

		[HttpGet("{id}/history")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetInvoiceHistory(long id)
		{
			_logger.LogInformation("Getting Invoice History");
			var history =await _service.GetInvoiceHistoryAsync(id);

			var result = history.Select(i => new InvoiceResponse(
				i.Id,
				i.Amount.ToString("F2"),
				i.PaidAmount.ToString("F2"),
				i.DueDate,
				i.Status.ToString()
			));

			return Ok(result);
		}

		[HttpGet("{id}/root")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetRootInvoice(long id)
		{
			_logger.LogInformation("Getting Root Invoice");
			var invoice = await _service.GetRootInvoiceAsync(id);

			var result = new InvoiceResponse(
				invoice.Id,
				invoice.Amount.ToString("F2"),
				invoice.PaidAmount.ToString("F2"),
				invoice.DueDate,
				invoice.Status.ToString()
			);

			return Ok(result);
		}

		[HttpGet("{id}/current")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetLatestActiveInvoice(long id)
		{
			_logger.LogInformation("Getting Latest Active Invoice");
			var invoice = await _service.GetLatestActiveInvoiceAsync(id);

			if (invoice == null)
				return NotFound("No active invoice found.");

			var result = new InvoiceResponse(
				invoice.Id,
				invoice.Amount.ToString("F2"),
				invoice.PaidAmount.ToString("F2"),
				invoice.DueDate,
				invoice.Status.ToString()
			);

			return Ok(result);
		}

		[HttpPost("{id}/payments_new")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> AddPaymentNew(long id, PaymentRequest request)
		{
			_logger.LogInformation("Applying Payment to Latest Active Invoice");
			await _service.AddPaymentNewAsync(id, request.Amount);
			return Ok(new { message = "Payment applied to latest active invoice." });
		}

		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteInvoice(long id)
		{
			_logger.LogInformation("Deleting Invoice");
			await _service.DeleteInvoiceAsync(id);
			return NoContent();
		}
	}
}
