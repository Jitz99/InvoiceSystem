namespace InvoiceSystem.Api.Dtos
{
	public class ProcessOverdueRequest
	{
		public decimal LateFee { get; set; }
		public int OverdueDays { get; set; }
	}
}
