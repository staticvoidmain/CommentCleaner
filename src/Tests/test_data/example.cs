namespace MySuperSweetNamspace {
	using System; // super critical
	using System.Text.RegularExpressions;

	/// <summary>Provides a nice smattering of comments</summary>
	public class Example {
		private readonly string name = "\"//Selector\"";
		private static readonly string xpath = "/*";
		private static readonly Regex re = new Regex(@"^(//.*/)+$");
		
		void DoAllTheFoos() {
			for(int i = 0; i < i * 2; i += 3) {
				// DoFoo();
				DoBar(); /* some jerk commented code just there */
			}
		}
	}
}