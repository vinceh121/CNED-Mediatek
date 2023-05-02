using Mediatek.Mapper;

namespace Mediatek.Entities
{
	public record Service(
		[DbAttribute("idservice")] long Id,
		[DbAttribute("nom")] string Name
	)
	{ }
}
