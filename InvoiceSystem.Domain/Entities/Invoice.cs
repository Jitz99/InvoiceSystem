using InvoiceSystem.Domain.Enums;
using System.Threading;

namespace InvoiceSystem.Domain.Entities;

public class Invoice
{
	public long Id { get; private set; }
	public decimal Amount { get; private set; }
	public decimal PaidAmount { get; private set; }
	public DateTime DueDate { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public InvoiceStatus Status { get; private set; }
	public long? ParentInvoiceId { get; private set; }
	public long RootInvoiceId { get; private set; }
	public bool IsDeleted { get; private set; }

	public decimal RemainingAmount => Amount - PaidAmount;
	public bool IsChildInvoice => ParentInvoiceId != null;
	public string DisplayStatus => Status switch
	{
		InvoiceStatus.Paid => "paid",
		InvoiceStatus.Void => "void",
		_ => "pending"
	};

	private Invoice() { } // For EF later

	//public Invoice(decimal amount, DateTime dueDate)
	//{
	//	if (amount <= 0)
	//		throw new ArgumentException("Amount must be greater than zero.");

	//	if (dueDate <= DateTime.UtcNow.Date)
	//		throw new ArgumentException("Due date must be in the future.");

	//	Id = Guid.NewGuid();
	//	Amount = amount;
	//	DueDate = dueDate;
	//	CreatedAt = DateTime.UtcNow;
	//	PaidAmount = 0;
	//	Status = InvoiceStatus.NotPaid;
	//}
	public Invoice(long id, decimal amount, DateTime dueDate, long? parentInvoiceId = null, long? rootInvoiceId = null)
	{
		if (id < 1000 && id != 0)
			throw new ArgumentException("Invoice Id must be at least 4 digits.");

		if (amount <= 0)
			throw new ArgumentException("Amount must be greater than zero.");

		Id = id;
		Amount = amount;
		DueDate = dueDate;
		CreatedAt = DateTime.UtcNow;

		PaidAmount = 0;
		Status = InvoiceStatus.NotPaid;

		if (parentInvoiceId == id)
			throw new ArgumentException("Invoice cannot be its own parent.");

		ParentInvoiceId = parentInvoiceId;

		if (parentInvoiceId == null)
		{
			// Root invoice
			RootInvoiceId = id;
		}
		else
		{
			RootInvoiceId = rootInvoiceId ?? throw new ArgumentException("RootInvoiceId required for child invoice.");
		}
	}


	public void SetId(long id)
	{
		if (Id != 0)
			throw new InvalidOperationException("Id already assigned.");

		Id = id;

		if (RootInvoiceId == 0)
			RootInvoiceId = id;
	}
	public void AddPayment(decimal paymentAmount)
	{
		EnsureNotDeleted();
		if (Status == InvoiceStatus.Paid)
			throw new InvalidOperationException("Invoice already paid.");

		if (paymentAmount <= 0)
			throw new ArgumentException("Payment must be greater than zero.");

		if (PaidAmount + paymentAmount > Amount)
			throw new InvalidOperationException("Payment exceeds invoice amount.");

		PaidAmount += paymentAmount;

		UpdateStatus();
	}

	//public bool IsOverdue(int overdueDays)
	//{
	//	if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Void)
	//		return false;

	//	var today = DateOnly.FromDateTime(DateTime.UtcNow);
	//	var dueDate = DateOnly.FromDateTime(DueDate);

	//	return today > dueDate.AddDays(overdueDays);
	//}

	public void MarkVoid()
	{
		EnsureNotDeleted();
		EnsureNotPaid();
		EnsureNotVoid();

		Status = InvoiceStatus.Void;
	}

	public void MarkPaid()
	{
		EnsureNotDeleted();
		EnsureNotPaid();
		EnsureNotVoid();

		Status = InvoiceStatus.Paid;
	}

	public void MarkDeleted()
	{
		EnsureNotDeleted();

		IsDeleted = true;
	}

	private void EnsureNotPaid()
	{
		if (Status == InvoiceStatus.Paid)
			throw new InvalidOperationException("Invoice is already paid.");
	}

	private void EnsureNotVoid()
	{
		if (Status == InvoiceStatus.Void)
			throw new InvalidOperationException("Invoice is already void.");
	}

	private void UpdateStatus()
	{
		if (PaidAmount == 0)
			Status = InvoiceStatus.NotPaid;
		else if (PaidAmount < Amount)
			Status = InvoiceStatus.PartiallyPaid;
		else
			Status = InvoiceStatus.Paid;
	}

	public bool IsEligibleForOverdue(DateTime utcNow, int overdueDays)
	{
		if (overdueDays < 0)
			throw new ArgumentException("Overdue days must be non-negative.");

		if (Status != InvoiceStatus.NotPaid &&
			Status != InvoiceStatus.PartiallyPaid)
			return false;

		var today = DateOnly.FromDateTime(utcNow);
		var dueDate = DateOnly.FromDateTime(DueDate);

		return today > dueDate.AddDays(overdueDays);
	}


	public void Cancel()
	{
		MarkVoid();
	}

	private void EnsureNotDeleted()
	{
		if (IsDeleted)
			throw new InvalidOperationException("Invoice is deleted.");
	}

}