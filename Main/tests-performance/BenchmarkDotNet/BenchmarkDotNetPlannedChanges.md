## layered design: Bench.NET part

  * In-process toolchain. Should be included in Bench.Net.
      + validates that job matches to the environment. Fails if do not math.
      + reports execution results as???

  * Bench.Net competition validators:
      + should be run as fast as possible (validation failure should be reported in less than 100ms).

  * In-process analysers. Should be included in Bench.Net. Use case: CompetitionLimitsAnalyser.
      + collects memory consumption / GC count to be used by Competition validation layer
      + stores data in reusable format

## layered design: BanchmarkDotNet.Competition part

  * CompetitionConfig (user settable):
      + annotation policy, rerun policy, default competition limits. Immutable for infrastructure code

  * CompetitionState (depends on nothing). All inside is passive. Should be implemented as Dictionary{guid, object}. Stores:
      + CompetitionConfig.
      + Execution engine parameters - request to rerun the benchmark + check if the current run is last one.
      + IMessages.
      + CompetitionTargets collection.

  * IMessage stack (stored as part of CompetitionState).
      + Stores messages to be reported to the user after benchmark run.
      + DOES NOT stores messages that relates only to particular run.
      + Helpers to break execution on execution errors?

  * Competition attributes (depends on nothing).
      + Allows to setup individual competition limits per each benchmark

  * CompetitionTargets (stored as part of CompetitionState). Immutalbe.
      + Stores actual competition limits per each benchmark

  * Competition validation layer (depends on CompetitionConfig + Competition attributes + CompetitionTargets + Execution engine parameters)
      + if enabled in CompetitionConfig: At first run initialises CompetitionTargets from source or from xml resource file (depends on Competition attributes)
      + Validates Summary after each run.
      + if enabled in CompetitionConfig: Requests rerun if validation failed

  * Competition annotation layer (depends on Competition validation + CompetitionTargets + Execution engine parameters + PDB API)
      + Is run before competition validation.
      + Extends CompetitionTargets to fit the actual results
      + Annotates the source (or xml resource file) with the updated CompetitionTargets.

  * competition runner
      + provides extensibility points for CompetitionState
      + allows to run arbitrary code before each benchmark run
      + allows to run arbitrary code after each benchmark run
      + allows to rerun the benchmark
	  + extracts messages from analysers/validators into IMessages

  * Unit-test competition runner: 
      + setups the runner depending on user config, enables Competition validation & Competition annotation layers
      + handles various quirks related to current directory, output etc
      + reports warnings / errors to user


