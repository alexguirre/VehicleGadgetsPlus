using System.Reflection;
using System.Runtime.InteropServices;
using Rage.Attributes;

[assembly: AssemblyTitle("Vehicle Gadgets+")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("alexguirre")]
[assembly: AssemblyProduct("Vehicle Gadgets+")]
[assembly: AssemblyCopyright("Copyright ©  $CR_YEAR$ alexguirre")] // set by AppVeyor
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("32b66d4a-850b-455b-a1c2-fb0a2d221218")]
[assembly: AssemblyVersion("0.0.0.0")] // set by AppVeyor
[assembly: AssemblyFileVersion("0.0.0.0")] // set by AppVeyor
[assembly: AssemblyInformationalVersion("0.0")] // set by AppVeyor
[assembly: Plugin("Vehicle Gadgets+", Author = "alexguirre", PrefersSingleInstance = true)]
