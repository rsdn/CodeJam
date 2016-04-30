# List of optimizarions to do:

* Agressive inlining and prooftest for it
* Non-readonly fields:https://codeblog.jonskeet.uk/2014/07/16/micro-optimization-the-surprising-inefficiency-of-readonly-fields/
* DO WE NEED IT? factory methods for Nullable<T> => Range<T>; 
  Good part: ~2x speedup?
  Bad part: GetValueOrDefault for infinity will return 0, not null.