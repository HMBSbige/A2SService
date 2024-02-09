global using A2SService;
global using FakeA2SServer;
global using JetBrains.Annotations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using Serilog.Events;
global using System.Net;
global using Volo.Abp;
global using Volo.Abp.Autofac;
global using Volo.Abp.DependencyInjection;
global using Volo.Abp.Modularity;

namespace FakeA2SServer;

[DependsOn(
	typeof(AbpAutofacModule)
)]
[UsedImplicitly]
internal class FakeA2SServerModule : AbpModule;
