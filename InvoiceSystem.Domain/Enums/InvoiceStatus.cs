using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSystem.Domain.Enums
{
	public enum InvoiceStatus
	{
		NotPaid = 0,
		PartiallyPaid = 1,
		Paid = 2,
		Void = 3
	}
}
	