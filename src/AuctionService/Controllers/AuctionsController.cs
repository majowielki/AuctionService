using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _auctionDbContext;
        private readonly IMapper _mapper;
        public readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext auctionDbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _auctionDbContext = auctionDbContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _auctionDbContext.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrWhiteSpace(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _auctionDbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }
            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            auction.Seller = "alice";

            _auctionDbContext.Auctions.Add(auction);

            var result = await _auctionDbContext.SaveChangesAsync() > 0;

            var newAuction = _mapper.Map<AuctionDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            if (!result)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _auctionDbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _auctionDbContext.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _auctionDbContext.Auctions.FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }

            _auctionDbContext.Remove(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _auctionDbContext.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not update DB");
            }

            return Ok();
        }
    }
}
