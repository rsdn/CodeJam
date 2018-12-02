using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Base class for annotation documents
	/// </summary>
	internal abstract class AnnotationDocument: IDisposable
	{
		/// <summary>Initializes a new instance of the <see cref="AnnotationDocument"/> class.</summary>
		/// <param name="origin">Location of the document.</param>
		/// <param name="parsed">If set to <c>true</c> the document is parsed successfully and can be updated with new annotations.</param>
		protected AnnotationDocument([NotNull] string origin, bool parsed)
		{
			Code.NotNullNorEmpty(origin, nameof(origin));
			Origin = origin;
			Parsed = parsed;
		}

		/// <summary>Gets location of the document.</summary>
		/// <value>The location of the document.</value>
		[NotNull]
		public string Origin { get; }

		/// <summary>Gets a value indicating whether the document is parsed successfully and can be updated with new annotations.</summary>
		/// <value><c>true</c> if the document is parsed successfully; otherwise, <c>false</c>.</value>
		public bool Parsed { get; private set; }

		/// <summary>Gets or sets annotation context the document belongs to.</summary>
		/// <value>The annotation context.</value>
		public AnnotationContext AnnotationContext { get; internal set; }

		/// <summary>Gets a value indicating whether the document has unsaved changes.</summary>
		/// <value>
		/// <c>true</c> if the document has changes to save; otherwise, <c>false</c>.
		/// </value>
		public bool HasChanges { get; private set; }

		/// <summary>Ensures that the document is parsed successfully and can be updated with new annotations.</summary>
		[AssertionMethod]
		protected void AssertIsParsed()
		{
			if (!Parsed)
				throw CodeExceptions.InvalidOperation($"Trying to change non-parsed document {Origin}.");
		}

		/// <summary>Marks the document as changed.</summary>
		protected void MarkAsChanged()
		{
			AssertIsParsed();
			HasChanges = true;
		}

		/// <summary>Saves the document if it was changed.</summary>
		public void Save()
		{
			if (!Parsed)
				throw CodeExceptions.InvalidOperation($"Trying to save non-parsed document {Origin}.");

			if (!HasChanges)
				return;

			SaveCore();
			HasChanges = false;
		}

		/// <summary>Saves the document.</summary>
		protected abstract void SaveCore();

		#region IDisposable
		/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (!Parsed)
				return;

			Dispose(true);
			GC.SuppressFinalize(this);
			Parsed = false;
		}

		/// <summary>Finalizes an instance of the <see cref="AnnotationDocument"/> class.</summary>
		~AnnotationDocument() {
			Dispose(false);
		}
		#endregion
	}
}