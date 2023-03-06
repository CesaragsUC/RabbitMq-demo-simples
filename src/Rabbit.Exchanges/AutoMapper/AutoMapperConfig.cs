using AutoMapper;
using Rabbit.Exchanges.Models;
using Rabbit.Subscription.Domain;
using RabbitAppContext;

namespace Rabbit.Exchanges.AutoMapper
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig() 
        { 
            CreateMap<Cliente,ClienteModel>().ReverseMap();
        }   
    }
}
