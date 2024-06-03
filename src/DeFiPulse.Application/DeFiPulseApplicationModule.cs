
using DeFiPulse.Project;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace DeFiPulse
{
    [DependsOn(
        typeof(DeFiPulseDomainModule),
        typeof(DeFiPulseApplicationContractsModule),
        typeof(AbpLocalizationModule),
        typeof(AbpVirtualFileSystemModule)
    )]
    public class DeFiPulseApplicationModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<ApiConfigOptions>(configuration.GetSection("ApiConfig"));
        }
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<DeFiPulseApplicationModule>();
            });
            context.Services.AddSingleton<OtherTokenSendContractService>();
            context.Services.AddSingleton<NftSeedTokenSendContractService>();

            context.Services.AddSingleton<ITokenSendContractService>(sp => sp.GetRequiredService<OtherTokenSendContractService>());
            context.Services.AddSingleton<ITokenSendContractService>(sp => sp.GetRequiredService<NftSeedTokenSendContractService>());
        }
    }
}
