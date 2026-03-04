namespace InvoiceSystem.Api.Dtos
{
	public class CreateInvoiceRequest
	{
		public decimal Amount { get; set; }
		public DateOnly DueDate { get; set; }
	}
}
