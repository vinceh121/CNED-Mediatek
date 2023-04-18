using System;
using Mediatek.Mapper;

namespace Mediatek.Entities
{
	public record Leave(
		[DbAttribute("idabsence")] long Id,
		[DbAttribute("datedebut")] DateTime Start,
		[DbAttribute("datefin")] DateTime End,
		[DbAttribute("idpersonnel")] long IdStaff,
		[DbAttribute("idmotif")] long IdReason
	)
	{
		public Staff Staff { get; set; }
		public Reason Reason { get; set; }
	}
}
