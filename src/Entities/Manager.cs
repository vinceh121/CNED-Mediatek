using Mediatek.Mapper;

namespace Mediatek.Entities
{
	public record Manager(
		[DbAttribute("login")] string Login,
		[DbAttribute("pwd")] string Password
	)
	{ }
}
