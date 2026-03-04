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
		public void CreateInvoice_ShouldCallRepositoryAdd()
		{
			var dueDate = DateTime.UtcNow.AddDays(5);

			_service.CreateInvoice(200, dueDate);

			_repositoryMock.Verify(
				r => r.Add(It.IsAny<Invoice>()),
				Times.Once);
		}

		[Fact]
		public void AddPayment_ShouldUpdateInvoice()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(5));

			_repositoryMock.Setup(r => r.GetById(invoice.Id))
				.Returns(invoice);

			_service.AddPayment(invoice.Id, 50);

			invoice.PaidAmount.Should().Be(50);

			_repositoryMock.Verify(r => r.Update(invoice), Times.Once);
		}

		[Fact]
		public void AddPayment_ShouldThrowException_WhenPaymentExceedsAmount()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(5));

			_repositoryMock.Setup(r => r.GetById(invoice.Id))
				.Returns(invoice);

			Action act = () => _service.AddPayment(invoice.Id, 150);

			act.Should().Throw<InvalidOperationException>()
				.WithMessage("Payment exceeds invoice amount.");

			_repositoryMock.Verify(r => r.Update(It.IsAny<Invoice>()), Times.Never);
		}

		[Fact]
		public void ProcessOverdue_ShouldCreateNewInvoice_WhenUnpaid()
		{
			var oldInvoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(-20));

			var invoices = new List<Invoice> { oldInvoice };

			_repositoryMock.Setup(r => r.GetAll()).Returns(invoices);

			_service.ProcessOverdue(10, 5);

			_repositoryMock.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Once);
			oldInvoice.Status.Should().Be(InvoiceStatus.Void);
		}

		[Fact]
		public void ProcessOverdue_ShouldNotCreateInvoice_WhenAlreadyPaid()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(-20));
			invoice.AddPayment(100); // fully paid

			var invoices = new List<Invoice> { invoice };

			_repositoryMock.Setup(r => r.GetAll()).Returns(invoices);

			_service.ProcessOverdue(10, 5);

			_repositoryMock.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Never);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}

		[Fact]
		public void ProcessOverdue_ShouldCreateInvoice_WhenPartiallyPaid()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(-20));
			invoice.AddPayment(40); // partial payment

			var invoices = new List<Invoice> { invoice };

			_repositoryMock.Setup(r => r.GetAll()).Returns(invoices);

			_service.ProcessOverdue(10, 5);

			_repositoryMock.Verify(r => r.Add(It.IsAny<Invoice>()), Times.Once);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}

		[Fact]
		public void GetAllInvoices_ShouldReturnInvoices()
		{
			var invoices = new List<Invoice>
	{
		new Invoice(0, 100, DateTime.UtcNow.AddDays(5)),
		new Invoice(0, 200, DateTime.UtcNow.AddDays(10))
	};

			_repositoryMock
				.Setup(r => r.GetAll())
				.Returns(invoices);

			var result = _service.GetAllInvoices();

			result.Should().HaveCount(2);
			result.Should().BeEquivalentTo(invoices);

			_repositoryMock.Verify(r => r.GetAll(), Times.Once);
		}
	}
}
