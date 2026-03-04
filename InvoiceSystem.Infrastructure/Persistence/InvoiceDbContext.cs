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
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Invoice>()
				.Property(i => i.Amount)
				.HasPrecision(18, 2);

			modelBuilder.Entity<Invoice>()
				.Property(i => i.PaidAmount)
				.HasPrecision(18, 2);
		}
	}
}
