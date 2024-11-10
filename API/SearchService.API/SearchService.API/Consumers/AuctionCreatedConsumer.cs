using MassTransit;
using Contracts;
using AutoMapper;
using SearchService.API.Models;
using MongoDB.Entities;

namespace SearchService.API.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);



            await item.SaveAsync();
        }
    }
}
