# ![logo](nuget/CodeJam.PerfTests.Icon.png) CodeJam.PerfTests

## What is it? (short version)

CodeJam.PerfTests is a framework for .net apps aimed to make perfomance testing as easy as normal unit tests are (NUnit, xUnit and MsTest test frameworks are supported right now, others - as requested). 

The goal of perftest is to proof your code will not break performance contract no matter under what conditions it is being run. To achieve that CodeJam.PerfTests collects, stores and validates various performance metrics during each perf test run.

In addition there is a set of features that are essential for real-world projects. To name a few: metric limits auto-adjustment, Continuous Integration support, support for perftests over dynamically emitted code and so on.



## Things to read

* [Intro](docs/Intro.md)
* [Overview](docs/Overview.md)
  * [Source Annotations](docs/SourceAnnotations.md) 
  * [Configuration System](docs/ConfigurationSystem.md)
  * [Competition Metric API](docs/CompetitionMetrics.md)
* [Tutorial](docs/Tutorial.md) (in progress).


## Contributing

Nothing special, use common [CONTRIBUTING guide](../CONTRIBUTING.md) for the CodeJam project.



> **WANTED: REWRITERS!**
>
> As you may have mention, I'm not native English speaker and docs sometimes are... weird. If you dislike the texts as much as I do and you want to improve them - you're welcome!
>
> All documentation files are [here](https://github.com/rsdn/CodeJam/tree/master/PerfTests/docs). Feel free to do PR or just [create an issue](https://github.com/rsdn/CodeJam/issues) (to notify you're fixing the document), download, edit and attach fixed file(s) into issue.



## Thanks to

Sorry if I forgot to mention you. [Let me know](https://github.com/rsdn/CodeJam/issues):) 

In alphabetical order, Greece enters the stadium first:

* [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) team. [@adamsitnik](https://github.com/adamsitnik), [@AndreyAkinshin](https://github.com/AndreyAkinshin), [@mattwarren](https://github.com/mattwarren) - thanks!!!
* [AppVeyor CI](https://www.appveyor.com) and personal to [@FeodorFitsner](https://github.com/FeodorFitsner). It just works and when it is not there's best support we have tried so far.
* [NUnit project](https://www.nunit.org/) and especially to [@CharliePoole](https://github.com/CharliePoole). Almost perfect.
* RSDN.org member Arthur Kozyrev for CodeJam logos family.
* RSDN.org members [@Буравчик](http://rsdn.org/account/info/58047) & [@Lexey](http://rsdn.org/account/info/460): [lognormal distribution for relative timings proposal](http://rsdn.org/forum/alg/6471574) (RUS). (Notify me if I should use another links to your profiles!)
* [@simon-mourier](http://stackoverflow.com/users/403671/simon-mourier) for helping [with pdb checksum verification](http://stackoverflow.com/q/36649271).

