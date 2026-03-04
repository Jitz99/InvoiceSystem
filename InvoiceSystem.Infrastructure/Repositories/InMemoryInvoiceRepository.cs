using System;
using System.Collections.Concurrent;
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
	public class InMemoryInvoiceRepository : IInvoiceRepository
	{
		private readonly ILogger<InMemoryInvoiceRepository> _logger;
		private readonly ConcurrentDictionary<long, Invoice> _storage = new();
		private long _currentId = 1000;

		public InMemoryInvoiceRepository(
		ILogger<InMemoryInvoiceRepository> logger)
		{
			_logger = logger;
		}
		public async Task AddAsync(Invoice invoice)
		{
			_logger.LogInformation("Inserting invoice into in-memory store");
			if (invoice.Id == 0)
			{
				invoice.SetId(await GenerateId());
			}
			_storage[invoice.Id] = invoice;
			_logger.LogInformation("Invoice added to in-memory store {InvoiceId}", invoice.Id);
		}
		public async Task<long> GenerateId()
		{
			var id = Interlocked.Increment(ref _currentId);
			_logger.LogInformation("Generated new invoice id {InvoiceId}", id);
			return id;

		}

		public async Task<Invoice?> GetByIdAsync(long id)
		{
			_logger.LogInformation("Fetching invoice {InvoiceId}", id);
			_storage.TryGetValue(id, out var invoice);
			return invoice;
		}

		public async Task<IEnumerable<Invoice>> GetAllAsync()
		{
			_logger.LogInformation("Fetching all invoices from in-memory store");
			return _storage.Values
			.Where(i => !i.IsDeleted);
		}

		public async Task UpdateAsync(Invoice invoice)
		{
			_storage[invoice.Id] = invoice;
			_logger.LogInformation("Invoice updated {InvoiceId}", invoice.Id);
		}
	}
}
