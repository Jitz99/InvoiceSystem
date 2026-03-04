using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Infrastructure.Persistence
{
	public class InvoiceDbContext : DbContext
	{
		public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options)
			: base(options) { }

		public DbSet<Invoice> Invoices => Set<Invoice>();
	}
}
