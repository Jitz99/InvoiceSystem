using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Tests.Domain
{
	public class InvoiceTests
	{
		[Fact]
		public void CreateInvoice_ShouldInitializeCorrectValues()
		{
			var dueDate = DateTime.UtcNow.AddDays(10);

			var invoice = new Invoice(0, 100, dueDate);

			invoice.Amount.Should().Be(100);
			invoice.PaidAmount.Should().Be(0);
			invoice.Status.Should().Be(InvoiceStatus.NotPaid);
		}

		[Fact]
		public void AddPayment_ShouldUpdatePaidAmount()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(5));

			invoice.AddPayment(40);

			invoice.PaidAmount.Should().Be(40);
			invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
		}

		[Fact]
		public void AddPayment_ShouldMarkInvoicePaid_WhenFullyPaid()
		{
			var invoice = new Invoice(0, 100, DateTime.UtcNow.AddDays(5));

			invoice.AddPayment(100);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}
		[Fact]
		public void Constructor_ShouldCreateRootInvoice_WhenNoParent()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.Id.Should().Be(1001);
			invoice.RootInvoiceId.Should().Be(1001);
			invoice.ParentInvoiceId.Should().BeNull();
			invoice.Status.Should().Be(InvoiceStatus.NotPaid);
		}
		[Fact]
		public void Constructor_ShouldCreateChildInvoice_WhenParentProvided()
		{
			var invoice = new Invoice(
				1002,
				150,
				DateTime.UtcNow.AddDays(5),
				parentInvoiceId: 1001,
				rootInvoiceId: 1001);

			invoice.ParentInvoiceId.Should().Be(1001);
			invoice.RootInvoiceId.Should().Be(1001);
			invoice.IsChildInvoice.Should().BeTrue();
		}
		[Fact]
		public void AddPayment_ShouldUpdateStatusToPartiallyPaid()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.AddPayment(40);

			invoice.PaidAmount.Should().Be(40);
			invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
		}

		[Fact]
		public void AddPayment_ShouldMarkInvoicePaid_WhenAmountReached()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.AddPayment(100);

			invoice.Status.Should().Be(InvoiceStatus.Paid);
		}

		[Fact]
		public void AddPayment_ShouldThrow_WhenPaymentExceedsAmount()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			Action act = () => invoice.AddPayment(120);

			act.Should().Throw<InvalidOperationException>()
				.WithMessage("Payment exceeds invoice amount.");
		}
		[Fact]
		public void Cancel_ShouldMarkInvoiceVoid()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.Cancel();

			invoice.Status.Should().Be(InvoiceStatus.Void);
		}

		[Fact]
		public void Cancel_ShouldThrow_WhenInvoicePaid()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));
			invoice.AddPayment(100);

			Action act = () => invoice.Cancel();

			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void MarkDeleted_ShouldSetIsDeleted()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.MarkDeleted();

			invoice.IsDeleted.Should().BeTrue();
		}

		//[Fact]
		//public void IsOverdue_ShouldReturnTrue_WhenPastDue()
		//{
		//	var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(-10));

		//	var result = invoice.IsEligibleForOverdue(DateTime.UtcNow, 5);

		//	result.Should().BeTrue();
		//}

		[Fact]
		public void DisplayStatus_ShouldReturnPending_WhenNotPaid()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.DisplayStatus.Should().Be("pending");
		}

		[Fact]
		public void Constructor_ShouldThrow_WhenAmountIsZeroOrNegative()
		{
			Action act = () => new Invoice(1001, 0, DateTime.UtcNow.AddDays(5));

			act.Should().Throw<ArgumentException>();
		}
		[Fact]
		public void MarkPaid_ShouldThrow_WhenInvoiceVoid()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));

			invoice.MarkVoid();

			Action act = () => invoice.MarkPaid();

			act.Should().Throw<InvalidOperationException>();
		}
		[Fact]
		public void RemainingAmount_ShouldReturnCorrectValue()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(5));
			invoice.AddPayment(30);

			invoice.RemainingAmount.Should().Be(70);
		}

		[Fact]
		public void IsEligibleForOverdue_ShouldReturnTrue_WhenPastDue()
		{
			var invoice = new Invoice(1001, 100, DateTime.UtcNow.AddDays(-10));

			var result = invoice.IsEligibleForOverdue(DateTime.UtcNow, 5);

			result.Should().BeTrue();
		}

		[Fact]
		public void IsEligibleForOverdue_ShouldReturnFalse_WhenExactlyOnLimit()
		{
			var dueDate = DateTime.UtcNow.AddDays(-10);
			var invoice = new Invoice(1001, 100, dueDate);

			var result = invoice.IsEligibleForOverdue(DateTime.UtcNow, 10);

			result.Should().BeFalse();
		}
	}
}
