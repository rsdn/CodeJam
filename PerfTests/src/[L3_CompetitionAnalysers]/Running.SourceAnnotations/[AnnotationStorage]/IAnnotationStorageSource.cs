using System;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Base interface for annotation storage providers.
	/// </summary>
	internal interface IAnnotationStorageSource
	{
		/// <summary>Gets annotation storage.</summary>
		/// <value>The annotation storage.</value>
		IAnnotationStorage AnnotationStorage { get; }
	}
}