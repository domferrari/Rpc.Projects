namespace Rpc.TeX.Library
{
	public static class Utils
	{
		/// -----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// A null from date means today.
		/// </summary>
		/// -----------------------------------------------------------------------------------------------------------
		public static DateTime GetFollowingSunday(DateTime? from = null)
		{
			var date = from ?? DateTime.Now;

			while (date.DayOfWeek != DayOfWeek.Sunday)
				date = date.AddDays(1);

			return date;
		}
	}
}
