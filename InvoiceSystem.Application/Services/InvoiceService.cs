using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSystem.Application.Interfaces;
using InvoiceSystem.Application.Models;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InvoiceSystem.Application.Services
{
	public class InvoiceService : IInvoiceService
	{
		private readonly IInvoiceRepository _repository;
		private readonly ILogger<InvoiceService> _logger;

		public InvoiceService(IInvoiceRepository repository,
		ILogger<InvoiceService> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<long> CreateInvoiceAsync(decimal amount, DateTime dueDate)
		{
			//var id = _repository.GenerateId();
			var invoice = new Invoice(0, amount, dueDate);
			await _repository.AddAsync(invoice);
			return invoice.Id;
		}

		public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
		{
			_logger.LogInformation("Retrieving invoices from repository");
			return await _repository.GetAllAsync();
		}

		public async Task AddPaymentAsync(long invoiceId, decimal amount)
		{
			var invoice = await _repository.GetByIdAsync(invoiceId)
						  ?? throw new KeyNotFoundException("Invoice not found.");

			_logger.LogInformation("Adding Payment for invoice in repository");
			invoice.AddPayment(amount);
			_logger.LogInformation("Updating invoice in repository");

			await _repository.UpdateAsync(invoice);
		}

		//public void ProcessOverdue(decimal lateFee, int overdueDays)
		//{
		//	var today = DateTime.UtcNow.Date;

		//	var invoices = _repository.GetAll()
		//	.Where(i => i.IsEligibleForOverdue(today, overdueDays))
		//	.ToList();

		//	foreach (var invoice in invoices)
		//	{
		//		if (invoice.PaidAmount > 0 && invoice.PaidAmount < invoice.Amount)
		//		{
		//			// Partial payment case
		//			var remaining = invoice.RemainingAmount;
		//			invoice.AddPayment(remaining); // Mark as fully paid
		//			//var id = _repository.GenerateId();

		//			var newInvoice = new Invoice(
		//				0,
		//				remaining + lateFee,
		//				today.AddDays(overdueDays),
		//				invoice.Id,
		//				invoice.RootInvoiceId==0?invoice.Id:invoice.RootInvoiceId);

		//			_repository.Add(newInvoice);
		//		}
		//		else if (invoice.PaidAmount == 0)
		//		{
		//			// Unpaid case
		//			invoice.MarkVoid();
		//			//var id = _repository.GenerateId();

		//			var newInvoice = new Invoice(
		//				0,
		//				invoice.Amount + lateFee,
		//				today.AddDays(overdueDays),
		//				invoice.Id,
		//				invoice.RootInvoiceId == 0 ? invoice.Id : invoice.RootInvoiceId);

		//			_repository.Add(newInvoice);
		//		}

		//		_repository.Update(invoice);
		//	}
		//}

		public async Task ProcessOverdueAsync(decimal lateFee, int overdueDays)
		{
			if (lateFee < 0)
				throw new ArgumentException("Late fee cannot be negative.", nameof(lateFee));

			if (overdueDays <= 0)
				throw new ArgumentException("Overdue days must be greater than zero.", nameof(overdueDays));

			var today = DateTime.UtcNow.Date;

			_logger.LogInformation(
				"Processing overdue invoices. LateFee:{LateFee} OverdueDays:{OverdueDays}",
				lateFee,
				overdueDays);

			_logger.LogInformation("Processing overdue in repository");
			var invoices = (await _repository.GetAllAsync())
			.Where(i => i.IsEligibleForOverdue(today, overdueDays))
			.ToList();

			foreach (var invoice in invoices)
			{
				decimal newAmount;

				if (invoice.PaidAmount > 0 && invoice.PaidAmount < invoice.Amount)
				{
					// Partial payment case
					newAmount = invoice.RemainingAmount + lateFee;

					// Close original invoice
					_logger.LogInformation("Marking as paid, Id:{id}",invoice.Id);
					invoice.MarkPaid();
				}
				else
				{
					// Unpaid case
					newAmount = invoice.Amount + lateFee;

					_logger.LogInformation("Marking as void, Id:{id}", invoice.Id);
					invoice.MarkVoid();
				}

				var rootId = invoice.RootInvoiceId == 0 ? invoice.Id : invoice.RootInvoiceId;

				var newInvoice = new Invoice(
					0,
					newAmount,
					today.AddDays(overdueDays),
					parentInvoiceId: invoice.Id,
					rootInvoiceId: rootId
				);

				_logger.LogInformation("Creating new overdue invoice for parent {InvoiceId}", invoice.Id);
				await _repository.AddAsync(newInvoice);
				await _repository.UpdateAsync(invoice);
			}
		}

		public async Task CancelInvoiceAsync(long invoiceId)
		{
			var invoice =await  _repository.GetByIdAsync(invoiceId)
				?? throw new KeyNotFoundException("Invoice not found.");

			_logger.LogInformation("Cancelling invoice in repository");
			invoice.Cancel();
			_logger.LogInformation("Updating invoice in repository");
			await _repository.UpdateAsync(invoice);
		}

		//public IEnumerable<Invoice> GetPaymentPendingInvoices()
		//{
		//	_logger.LogInformation("Cancelling invoice in repository");
		//	return _repository.GetAll()
		//		.Where(i => i.Status == InvoiceStatus.NotPaid ||
		//					i.Status == InvoiceStatus.PartiallyPaid);
		//}

		//public IEnumerable<Invoice> GetPaidInvoices()
		//{
		//	return _repository.GetAll()
		//		.Where(i => i.Status == InvoiceStatus.Paid);
		//}

		public async Task<IEnumerable<Invoice>> SearchInvoicesFilterAsync(InvoiceSearchFilter filter)
		{
			_logger.LogInformation("Getting all invoice in repository");
			var query = (await _repository.GetAllAsync()).AsQueryable();
			_logger.LogInformation("querying the invoices in repository");

			if (!string.IsNullOrEmpty(filter.Status))
			{
				var statusLower = filter.Status.ToLowerInvariant();

				query = query.Where(i =>
					i.DisplayStatus.ToLowerInvariant() == statusLower);
			}
			else
			{
				return Enumerable.Empty<Invoice>();
			}

			if (filter.FromDate.HasValue)
				query = query.Where(i => DateOnly.FromDateTime(i.DueDate) >= filter.FromDate.Value);

			if (filter.ToDate.HasValue)
				query = query.Where(i => DateOnly.FromDateTime(i.DueDate) <= filter.ToDate.Value);

			return query.ToList();
		}
		public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(
		InvoiceSearchFilter filter,
		int page,
		int pageSize)
			{
			_logger.LogInformation("searching the invoices based on query in repository");

			var query = (await SearchInvoicesFilterAsync(filter)).AsQueryable();

				return query
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();
		}

		public async Task<IEnumerable<Invoice>> GetInvoiceHistoryAsync(long invoiceId)
		{
			var invoice =await  _repository.GetByIdAsync(invoiceId)
				?? throw new KeyNotFoundException("Invoice not found.");
			_logger.LogInformation("searching the invoicehistory in repository for id:{id}", invoiceId);

			var rootId = invoice.RootInvoiceId == 0 ? invoice.Id : invoice.RootInvoiceId;

			return (await _repository.GetAllAsync())
				.Where(i => i.RootInvoiceId == rootId || i.Id == rootId)
				.OrderBy(i => i.CreatedAt)
				.ToList();
		}

		public async Task<Invoice> GetRootInvoiceAsync(long invoiceId)
		{
			var invoice =await _repository.GetByIdAsync(invoiceId)
				?? throw new KeyNotFoundException("Invoice not found.");
			_logger.LogInformation("Getting root invoice in repository of id:{id}", invoiceId);

			var rootId = invoice.RootInvoiceId == 0 ? invoice.Id : invoice.RootInvoiceId;

			return await _repository.GetByIdAsync(rootId)
				?? throw new KeyNotFoundException("Root invoice not found.");
		}
		public async Task<Invoice?> GetLatestActiveInvoiceAsync(long invoiceId)
		{
			var invoice =await _repository.GetByIdAsync(invoiceId)
				?? throw new KeyNotFoundException("Invoice not found.");
			_logger.LogInformation("getting the latestActive invoice in repository of id:{id}", invoiceId);

			var rootId = invoice.RootInvoiceId == 0 ? invoice.Id : invoice.RootInvoiceId;

			return (await _repository.GetAllAsync())
				.Where(i => (i.RootInvoiceId == rootId || i.Id == rootId) &&
							i.Status != InvoiceStatus.Paid &&
							i.Status != InvoiceStatus.Void)
				.OrderByDescending(i => i.CreatedAt)
				.FirstOrDefault();
		}
		public async Task AddPaymentNewAsync(long invoiceId, decimal amount)
		{
			_logger.LogInformation("getting the latestActive invoice in repository of id:{id}", invoiceId);
			var latestInvoice =await GetLatestActiveInvoiceAsync(invoiceId);

			if (latestInvoice == null)
				throw new InvalidOperationException("No active invoice available for payment.");

			_logger.LogInformation("updating payment for the latestActive invoice in repository of id:{id}", latestInvoice.Id);
			latestInvoice.AddPayment(amount);

			_logger.LogInformation("Updating invoice in repository");
			await _repository.UpdateAsync(latestInvoice);
		}
		public async Task DeleteInvoiceAsync(long id)
		{
			var invoice =await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Invoice not found.");

			// Prevent deleting if it has a parent
			if (invoice.ParentInvoiceId != null)
				throw new InvalidOperationException("Cannot delete an invoice that has a parent.");

			// Prevent deleting if it has children
			var hasChildren =(await _repository.GetAllAsync())
				.Any(i => i.ParentInvoiceId == invoice.Id);

			if (hasChildren)
				throw new InvalidOperationException("Cannot delete an invoice that has child invoices.");

			_logger.LogInformation("Marking invoice in repository as deleted with id:{id}", id);
			invoice.MarkDeleted();

			_logger.LogInformation("Updating invoice in repository");
			await _repository.UpdateAsync(invoice);
		}

	}
}
