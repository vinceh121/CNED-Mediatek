namespace mediatek
{
	[System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Property,
		Inherited = false, AllowMultiple = false)]
	sealed class DbAttribute : System.Attribute
	{
		readonly string columnName;
		public bool Optional { get; set; }

		public DbAttribute(string columnName)
		{
			this.columnName = columnName;
		}

		public string ColumnName
		{
			get { return columnName; }
		}
	}
}
