using AutoMapper;

namespace AuctionService.API.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Entities.Auction, DTOs.AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Entities.Item, DTOs.AuctionDto>();
            CreateMap<DTOs.CreateAuctionDto, Entities.Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<DTOs.CreateAuctionDto, Entities.Item>();
        }
    }
}
