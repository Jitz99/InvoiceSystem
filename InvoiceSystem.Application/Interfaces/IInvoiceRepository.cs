using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Application.Interfaces
{
	public interface IInvoiceRepository
	{
		Task AddAsync(Invoice invoice);

		Task<Invoice?> GetByIdAsync(long id);

		Task<IEnumerable<Invoice>> GetAllAsync();

		Task UpdateAsync(Invoice invoice);
	}
}
