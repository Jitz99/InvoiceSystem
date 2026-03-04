namespace InvoiceSystem.Api.Dtos
{
	public class InvoiceResponse(
	long id,
	decimal amount,
	decimal paidAmount,
	DateTime dueDate,
	string status)
	{
		public long Id { get; init; } = id;
		public decimal Amount { get; init; } = amount;
		public decimal PaidAmount { get; init; } = paidAmount;
		public DateTime DueDate { get; init; } = dueDate;
		public string Status { get; init; } = status;
	}
}
