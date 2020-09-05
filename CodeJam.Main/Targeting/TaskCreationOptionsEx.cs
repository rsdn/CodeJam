using System.Threading.Tasks;

namespace CodeJam.Targeting
{
	/// <summary>Extended <see cref="TaskCreationOptions"/></summary>
	internal static class TaskCreationOptionsEx
	{
		/// <summary>TaskCreationOptions.RunContinuationsAsynchronously or explicit value if not supported by target platform.</summary>
#if NET46_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
		public const TaskCreationOptions RunContinuationsAsynchronously = TaskCreationOptions.RunContinuationsAsynchronously;
#else
		public const TaskCreationOptions RunContinuationsAsynchronously = (TaskCreationOptions)64;
#endif
	}
}