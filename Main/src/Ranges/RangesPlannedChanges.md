## List of optimizations to do:
* Examples !!!!
* DO WE NEED IT? Agressive inlining on struct constructors. Should measure first.
* DO WE NEED IT? factory methods for Nullable<T> => Range<T>;
  Good part: ~2x speedup?
  Bad part: GetValueOrDefault for infinity will return 0, not null.

## List of optimizations done:
* prooftest AggressiveInliningPerfTests +  AggressiveInlining annotations.
* Non-readonly fields:https://codeblog.jonskeet.uk/2014/07/16/micro-optimization-the-surprising-inefficiency-of-readonly-fields/

## List of fixes:
 * DO WE NEED IT? detect NaN
 * Resolve the circular dependencies with Range, Range<T> and RangeBoundary<T>.
 * Design flaw with type inference:
 ```
  Range.Create(Range.BoundaryFrom(1), Range.BoundaryFrom(1)) =>
	Range<RangeBoundaryFrom<int>>;
 ```
 * DO WE NEED IT? Fix naming for Empty boundaries. Alternative names: Void, None, EmptyRangeBoundary

## Documentation:
As proposed by Lexey: Document weird moments with .IsExclusive property for Infinity boundaries.