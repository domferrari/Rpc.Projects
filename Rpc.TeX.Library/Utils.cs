namespace Rpc.TeX.Library
{
	public static class Utils
	{
		/// -----------------------------------------------------------------------------------------------------------
		public static DateTime GetNextSunday()
		{
			var date = DateTime.Now;

			while (date.DayOfWeek != DayOfWeek.Sunday)
				date = date.AddDays(1);

			return date;
		}
	}
}
