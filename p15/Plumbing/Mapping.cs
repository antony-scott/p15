using AutoMapper;
using p15.Core.Models;
using p15.ViewModels;

namespace p15.Plumbing
{
    public static class Mapping
    {
        public static IMapper Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Core.Models.App, AppViewModel>();
                cfg.CreateMap<Core.Models.Barcode, BarcodeViewModel>();
                cfg.CreateMap<LogEntryModel, LogEntryViewModel>();
            });

            return config.CreateMapper();
        }
    }
}
