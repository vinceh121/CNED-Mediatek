namespace mediatek
{
	public record Staff(
		[DbAttribute("idpersonnel")] long Id,
		[DbAttribute("nom")] string LastName,
		[DbAttribute("prenom")] string FirstName,
		[DbAttribute("tel")] string Phone,
		[DbAttribute("mail")] string Email,
		[DbAttribute("idservice")] long IdService
	)
	{
		[DbAttribute("nomservice", Optional = true)]
		public string Service { get; set; }
	}
}
