using System;
using System.Runtime.InteropServices;

using CodeJam.PerfTests;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("08e84f45-2782-4d2e-85c0-fe86af491c94")]

[assembly: CompetitionFeatures(ContinuousIntegrationMode = false)]
[assembly: CompetitionConfigFactory(typeof(SelfTestConfigFactory))]
