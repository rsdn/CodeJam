using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Known source file languages
	/// </summary>
	internal enum SourceLanguage
	{
		/// <summary>Language is unknown.</summary>
		[UsedImplicitly]
		Unknown = 0,

		/// <summary>C#</summary>
		CSharp,

		/// <summary>Visual Basic</summary>
		VisualBasic
	}
}