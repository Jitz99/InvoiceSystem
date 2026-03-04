using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using InvoiceSystem.Application.Interfaces;
using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvoiceSystem.Tests.Application
{
	public class InvoiceServiceTests
	{
		private readonly Mock<IInvoiceRepository> _repositoryMock;
		private readonly Mock<ILogger<InvoiceService>> _loggerMock;
		private readonly InvoiceService _service;

		public InvoiceServiceTests()
		{
			_repositoryMock = new Mock<IInvoiceRepository>();
			_loggerMock = new Mock<ILogger<InvoiceService>>();

			_service = new InvoiceService(
				_repositoryMock.Object,
				_loggerMock.Object);
		}

		[Fact]
		public async Task CreateInvoice_ShouldCallRepositoryAdd()
		{
			var dueDate = DateTime.UtcNow.AddDays(5);

			await _service.CreateInvoiceAsync(200, dueDate);

			_repositoryMock.Verify(
				r => r.AddAsync(It.IsAny<Invoice>()),
				Times.Once);
		}

		[Fact]
		public async Task AddPayment_ShouldUpdateInvoice()
		{
			var invoice = new Invoice(1000, 100, DateTime.UtcNow.AddDays(5));

			_repositoryMock
				.Setup(r => r.GetByIdAsync(invoice.Id))
				.ReturnsAsync(invoice);

			await _service.AddPaymentAsync(invoice.Id, 50);

			invoice.PaidAmount.Should().Be(50);

			_repositoryMock.Verify(r => r.UpdateAsync(invoice), Times.Once);
		}

		[Fact]
		public async Task AddPayment_ShouldThrowException_WhenPaymentExceedsAmount()
		{
			var invoice = new Invoice(1000, 100, DateTime.UtcNow.AddDays(5));

			_repositoryMock
				.Setup(r => r.GetByIdAsync(invoice.Id))
				.ReturnsAsync(invoice);

			Func<Task> act = async () =>
				await _service.AddPaymentAsync(invoice.Id, 150);

			await act.Should()
				.ThrowAsync<InvalidOperationException>()
				.WithMessage("Payment exceeds invoice amount.");

			_repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
		}

		[Fact]
		public async Task ProcessOverdue_ShouldCreateNewInvoice_WhenUnpaid()
		{
			var oldInvoice = new Invoice(1000, 100, DateTime.UtcNow.AddDays(-20));

			var invoices = new List<Invoice> { oldInvoice };

			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ReturnsAsync(invoices);

			await _service.ProcessOverdueAsync(10, 5);

			_repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);

			oldInvoice.Status.Should().Be(InvoiceStatus.Void);
		}

		[Fact]
		public async Task ProcessOverdue_ShouldNotCreateInvoice_WhenAlreadyPaid()
		{
			var invoice = new Invoice(1000, 100, DateTime.UtcNow.AddDays(-20));
			invoice.AddPayment(100);

			var invoices = new List<Invoice> { invoice };

			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ReturnsAsync(invoices);

			await _service.ProcessOverdueAsync(10, 5);

			_repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Never);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}

		[Fact]
		public async Task ProcessOverdue_ShouldCreateInvoice_WhenPartiallyPaid()
		{
			var invoice = new Invoice(1000, 100, DateTime.UtcNow.AddDays(-20));
			invoice.AddPayment(40);

			var invoices = new List<Invoice> { invoice };

			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ReturnsAsync(invoices);

			await _service.ProcessOverdueAsync(10, 5);

			_repositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}

		[Fact]
		public async Task GetAllInvoices_ShouldReturnInvoices()
		{
			var invoices = new List<Invoice>
		{
			new Invoice(1000, 100, DateTime.UtcNow.AddDays(5)),
			new Invoice(1001, 200, DateTime.UtcNow.AddDays(10))
		};

			_repositoryMock
				.Setup(r => r.GetAllAsync())
				.ReturnsAsync(invoices);

			var result = await _service.GetAllInvoicesAsync();

			result.Should().HaveCount(2);
			result.Should().BeEquivalentTo(invoices);

			_repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
		}
	}
}
