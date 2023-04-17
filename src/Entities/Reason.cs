using Mediatek.Mapper;

namespace Mediatek.Entities
{
	public record Reason(
		[DbAttribute("idmotif")] long Id,
		[DbAttribute("libelle")] string label
	)
	{ }
}
