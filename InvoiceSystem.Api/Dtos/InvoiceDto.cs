using System.ComponentModel.DataAnnotations;

namespace InvoiceSystem.Api.Dtos
{
	public class InvoiceDto
	{
		public Guid Id { get; set; }
		[Range(0.01, 999999999)]
		public decimal Amount { get; set; }
		public decimal PaidAmount { get; set; }
		public DateOnly DueDate { get; set; }
		public string Status { get; set; }
	}
}
