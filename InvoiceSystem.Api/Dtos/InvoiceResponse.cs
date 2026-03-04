namespace InvoiceSystem.Api.Dtos
{
	public class InvoiceResponse(
	long id,
	string amount,
	string paidAmount,
	DateTime dueDate,
	string status)
	{
		public long Id { get; init; } = id;
		public string Amount { get; init; } = amount;
		public string PaidAmount { get; init; } = paidAmount;
		public DateTime DueDate { get; init; } = dueDate;
		public string Status { get; init; } = status;
	}
}
