using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSystem.Application.Interfaces;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceSystem.Infrastructure.Repositories
{
	public class SqliteInvoiceRepository : IInvoiceRepository
	{
		private readonly InvoiceDbContext _context;
		private readonly ILogger<SqliteInvoiceRepository> _logger;

		public SqliteInvoiceRepository(
			InvoiceDbContext context,
			ILogger<SqliteInvoiceRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task AddAsync(Invoice invoice)
		{
			_logger.LogInformation("Inserting invoice");

			await _context.Invoices.AddAsync(invoice);
			await _context.SaveChangesAsync();
		}

		public async Task<Invoice?> GetByIdAsync(long id)
		{
			_logger.LogInformation("Reading invoice {InvoiceId}", id);

			return await _context.Invoices
				.FirstOrDefaultAsync(i => i.Id == id);
		}

		public async Task<IEnumerable<Invoice>> GetAllAsync()
		{
			_logger.LogInformation("Reading invoices");

			return await _context.Invoices
				.Where(i => !i.IsDeleted)
				.ToListAsync();
		}

		public async Task UpdateAsync(Invoice invoice)
		{
			_logger.LogInformation("Updating invoice {InvoiceId}", invoice.Id);

			_context.Invoices.Update(invoice);

			await _context.SaveChangesAsync();
		}
	}
}
