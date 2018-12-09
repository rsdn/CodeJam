using System;
using System.Collections.Generic;
using System.Threading;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// The annotation context.
	/// IMPORTANT: Any access to the annotation context should be performed under appcontext lock.
	/// To prevent accidental damage of source files the code that uses the context should be invoked via
	/// <see cref="RunInContext{TResult}"/> method.
	/// Attempt to bypass the lock will result in exception.
	/// </summary>
	internal sealed class AnnotationContext : IDisposable
	{
		#region Helper types
		/// <summary>
		/// Annotation documents cache
		/// </summary>
		private class DocumentsCache : OwnedCollectionBase<AnnotationContext, string, AnnotationDocument>
		{
			private readonly Dictionary<AnnotationTargetKey, AnnotationDocument> _documentsByTargets =
				new Dictionary<AnnotationTargetKey, AnnotationDocument>();

			/// <summary>Initializes a new instance of the <see cref="DocumentsCache"/> class.</summary>
			/// <param name="owner">The owner for the collection.</param>
			public DocumentsCache([NotNull] AnnotationContext owner) : base(owner) { }

			/// <summary>Gets a key for the item.</summary>
			/// <param name="item">The item.</param>
			/// <returns>Key for the item.</returns>
			protected override string GetKey(AnnotationDocument item) =>
				item.Origin;

			/// <summary>Gets the owner of the item.</summary>
			/// <param name="item">The item.</param>
			/// <returns>Owner of the item.</returns>
			protected override AnnotationContext GetOwner(AnnotationDocument item) =>
				item.AnnotationContext;

			/// <summary>Sets the owner of the item.</summary>
			/// <param name="item">The item.</param>
			/// <param name="owner">The owner of the item.</param>
			protected override void SetOwner(AnnotationDocument item, AnnotationContext owner)
			{
				Code.AssertState(
					item.AnnotationContext == null && owner != null,
					"Cannot change annotation context of the document.");

				item.AnnotationContext = owner;
			}

			/// <summary>Gets the <see cref="AnnotationDocument"/> by descriptor key.</summary>
			/// <value>The <see cref="AnnotationDocument"/>.</value>
			/// <param name="key">The descriptor key.</param>
			/// <returns><see cref="AnnotationDocument"/> for the descriptor.</returns>
			public AnnotationDocument this[AnnotationTargetKey key] => _documentsByTargets[key];

			/// <summary>Determines whether the cache contains annotation document for the specified descriptor key.</summary>
			/// <param name="key">The descriptor key.</param>
			/// <returns><c>true</c> if the cache contains annotation document for the specified descriptor key; otherwise, <c>false</c>.</returns>
			public bool Contains(AnnotationTargetKey key) => _documentsByTargets.ContainsKey(key);

			/// <summary>Adds descriptor key for the document.</summary>
			/// <param name="annotationDocument">The annotation document.</param>
			/// <param name="key">The descriptor key.</param>
			public void AddTargetKey(AnnotationDocument annotationDocument, AnnotationTargetKey key)
			{
				Code.AssertArgument(
					annotationDocument.Parsed &&
						ReferenceEquals(annotationDocument.AnnotationContext, Owner),
					nameof(annotationDocument),
					"The document is not parsed or does not belongs to the context.");

				_documentsByTargets.Add(key, annotationDocument);
			}
		}

		/// <summary>
		/// Stub instance for not parsed annotation document.
		/// Should be used if origin of the document was not found.
		/// </summary>
		/// <seealso cref="CodeJam.PerfTests.Running.SourceAnnotations.AnnotationDocument" />
		private class UnknownOriginDocument : AnnotationDocument
		{
			/// <summary>Initializes a new instance of the <see cref="UnknownOriginDocument"/> class.</summary>
			/// <param name="origin">Location of the document.</param>
			public UnknownOriginDocument([NotNull] string origin) : base(origin, false) { }

			/// <summary>Saves the document.</summary>
			protected override void SaveCore()
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region Static members
		/// <summary>Placeholder string for unknown document origin.</summary>
		private const string UnknownOrigin = "<Unknown>";

		private readonly DocumentsCache _documentsCache;

		private readonly object _lockKey = new object(); 
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AnnotationContext"/> class.</summary>
		public AnnotationContext()
		{
			_documentsCache = new DocumentsCache(this);
		}

		[AssertionMethod]
		private void AssertIsInLock()
		{
			if (!Monitor.IsEntered(_lockKey))
				throw CodeExceptions.InvalidOperation(
					$"Please run the code using the {nameof(RunInContext)} method.");
		}

		/// <summary>Runs annotation code.</summary>
		/// <typeparam name="TResult">Type of result of annotation.</typeparam>
		/// <param name="annotationCallback">The annotation callback.</param>
		/// <returns>Result of annotation.</returns>
		public TResult RunInContext<TResult>([NotNull] Func<AnnotationContext, TResult> annotationCallback)
		{
			Code.NotNull(annotationCallback, nameof(annotationCallback));

			// TODO: something async-friendly?
			lock (_lockKey)
			{
				return annotationCallback(this);
			}
		}

		#region API for code being run in context.
		/// <summary>Tries to get annotation document by the origin.</summary>
		/// <param name="origin">Location of the document.</param>
		/// <returns>The annotation document or <c>null</c> if not found.</returns>
		[CanBeNull]
		public AnnotationDocument TryGetDocument([NotNull] string origin)
		{
			Code.NotNullNorEmpty(origin, nameof(origin));
			AssertIsInLock();

			return _documentsCache.Contains(origin) ? _documentsCache[origin] : null;
		}

		/// <summary>Tries to get annotation document by the origin.</summary>
		/// <param name="key">The descriptor key.</param>
		/// <returns>The annotation document or <c>null</c> if not found.</returns>
		[CanBeNull]
		public AnnotationDocument TryGetDocument(AnnotationTargetKey key)
		{
			AssertIsInLock();

			return _documentsCache.Contains(key) ? _documentsCache[key] : null;
		}

		/// <summary>
		/// Gets stub instance for not parsed annotation document.
		/// Should be used if origin of the document was not found.
		/// </summary>
		/// <returns>Stub instance for not parsed annotation document.</returns>
		[NotNull]
		public AnnotationDocument GetUnknownOriginDocument()
		{
			AssertIsInLock();

			if (_documentsCache.Contains(UnknownOrigin))
				return (UnknownOriginDocument)_documentsCache[UnknownOrigin];

			var result = new UnknownOriginDocument(UnknownOrigin);
			_documentsCache.Add(result);
			return result;
		}

		/// <summary>Adds document into cache.</summary>
		/// <param name="annotationDocument">The annotation document.</param>
		public void AddDocument([NotNull] AnnotationDocument annotationDocument)
		{
			Code.NotNull(annotationDocument, nameof(annotationDocument));
			AssertIsInLock();

			_documentsCache.Add(annotationDocument);
		}

		/// <summary>Adds descriptor key for the document.</summary>
		/// <param name="annotationDocument">The annotation document.</param>
		/// <param name="key">The descriptor key.</param>
		public void AddTargetKey([NotNull] AnnotationDocument annotationDocument, AnnotationTargetKey key)
		{
			Code.NotNull(annotationDocument, nameof(annotationDocument));
			AssertIsInLock();
			_documentsCache.AddTargetKey(annotationDocument, key);
		}

		/// <summary>Saves all changes in the context.</summary>
		public void Save()
		{
			AssertIsInLock();

			foreach (var sourceDocument in _documentsCache)
			{
				sourceDocument.Save();
			}
		} 
		#endregion

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			lock (_lockKey)
			{
				foreach (var doc in _documentsCache)
				{
					doc.Dispose();
				}
				_documentsCache.Clear();
			}
		}
	}
}