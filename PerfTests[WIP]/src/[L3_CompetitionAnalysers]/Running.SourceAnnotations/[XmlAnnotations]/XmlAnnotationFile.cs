using System.IO;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Xml annotation document.</summary>
	/// <seealso cref="AnnotationDocument" />
	internal sealed class XmlAnnotationFile : AnnotationDocument
	{
		/// <summary>Initializes a new instance of the <see cref="XmlAnnotationFile"/> class.</summary>
		/// <param name="path">The path.</param>
		/// <param name="xmlAnnotationDoc">The XML annotation document.</param>
		public XmlAnnotationFile([NotNull] string path, [CanBeNull] XDocument xmlAnnotationDoc) :
			base(path, xmlAnnotationDoc != null)
		{
			XmlAnnotationDoc = xmlAnnotationDoc;
			if (XmlAnnotationDoc != null)
			{
				XmlAnnotationDoc.Changed += OnXmlAnnotationDocChanged;
			}
		}

		/// <summary>Gets the XML annotation document.</summary>
		/// <value>The XML annotation document.</value>
		[CanBeNull]
		public XDocument XmlAnnotationDoc { get; private set; }

		private void OnXmlAnnotationDocChanged(object sender, XObjectChangeEventArgs e) => MarkAsChanged();

		/// <summary>Saves the document.</summary>
		protected override void SaveCore()
		{
			using (var fileStream = File.Create(Origin))
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				XmlAnnotationHelpers.Save(XmlAnnotationDoc, fileStream);
			}
		}

		/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (XmlAnnotationDoc != null)
				{
					XmlAnnotationDoc.Changed -= OnXmlAnnotationDocChanged;
					XmlAnnotationDoc = null;
				}
			}
		}

	}
}