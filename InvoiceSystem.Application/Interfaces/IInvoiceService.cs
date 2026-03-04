using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSystem.Application.Models;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Application.Interfaces
{
	public interface IInvoiceService
	{
		Task<long> CreateInvoiceAsync(decimal amount, DateTime dueDate);

		Task<IEnumerable<Invoice>> GetAllInvoicesAsync();

		Task AddPaymentAsync(long invoiceId, decimal amount);

		Task ProcessOverdueAsync(decimal lateFee, int overdueDays);

		Task CancelInvoiceAsync(long invoiceId);

		Task<IEnumerable<Invoice>> SearchInvoicesFilterAsync(InvoiceSearchFilter filter);

		Task<IEnumerable<Invoice>> SearchInvoicesAsync(
			InvoiceSearchFilter filter,
			int page,
			int pageSize);

		Task<IEnumerable<Invoice>> GetInvoiceHistoryAsync(long invoiceId);

		Task<Invoice> GetRootInvoiceAsync(long invoiceId);

		Task<Invoice?> GetLatestActiveInvoiceAsync(long invoiceId);

		Task AddPaymentNewAsync(long invoiceId, decimal amount);

		Task DeleteInvoiceAsync(long id);
	}
}
