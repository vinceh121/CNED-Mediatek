using System;
using Mediatek.Mapper;

namespace Mediatek.Entities
{
	public record Leave(
		DateTime Start,
		DateTime End,
		long IdStaff,
		long IdReason
	)
	{
		public Staff Staff { get; set; }
		public Reason Reason { get; set; }
	}
}
