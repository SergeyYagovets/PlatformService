using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataService.Http;

namespace PlatformService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PlatformsController : ControllerBase
	{
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(
			IPlatformRepo repository,
			IMapper mapper,
			ICommandDataClient commandDataClient,
			IMessageBusClient messageBusClient)
		{
			_repository = repository;
			_mapper = mapper;
			_commandDataClient = commandDataClient;
			_messageBusClient = messageBusClient;
        }

		[HttpGet]
		public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
		{

			var platformItem = _repository.GetAllPlatforms();

			return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
		}

		[HttpGet("{id}", Name = "GetPlatformById")]
		public ActionResult<PlatformReadDto> GetPlatformById(int id)
		{
			var platformItem = _repository.GetPlatformById(id);

			if (platformItem == null)
			{
				return NotFound();
			}
			return _mapper.Map<PlatformReadDto>(platformItem);
		}

		[HttpPost]
		public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
		{
			var platformModel = _mapper.Map<Platform>(platformCreateDto);
			_repository.CreatePlatform(platformModel);
			_repository.SaveChanges();

			var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);

			// Send Sync Message
			try
			{
				await _commandDataClient.SendPlatformToCommand(platformReadDto);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
			}

			// Send Async Message
			try
			{
				var platformPublichedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
				platformPublichedDto.Event = "Platform_Published";
				_messageBusClient.PublishNewPlatform(platformPublichedDto);
			}
			catch (Exception ex)
			{
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
		}
	}
}

