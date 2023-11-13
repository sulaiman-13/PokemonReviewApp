using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repositories;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public OwnerController(IOwnerRepository OwnerRepository,ICountryRepository countryRepository, IMapper mapper)
        {
            _ownerRepository = OwnerRepository;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<OwnerDto>))]
        public IActionResult GetOwners()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var categories = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwners());

            return Ok(categories);
        }
        [HttpGet("{ownerId}")]
        [ProducesResponseType(200, Type = typeof(OwnerDto))]
        [ProducesResponseType(400)]
        public IActionResult GetOwner([FromRoute] int ownerId)
        {
            if (!_ownerRepository.IsOwnerExist(ownerId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var owner = _mapper.Map<OwnerDto>(_ownerRepository.GetOwner(ownerId));
            return Ok(owner);
        }
        [HttpGet("{ownerId}/pokemon")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PokemonDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonsByOwner(int ownerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_ownerRepository.IsOwnerExist(ownerId))
                return NotFound();
            var pokemons = _mapper.Map<List<PokemonDto>>(_ownerRepository.GetPokemonsByOwner(ownerId));

            return Ok(pokemons);
        }


        [HttpGet("pokemons/{pokemonId}/owners")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<OwnerDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetOwnersOfAPokemon(int pokemonId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var owners = _mapper.Map<List<OwnerDto>>(_ownerRepository.GetOwnersOfAPokemon(pokemonId));

            return Ok(owners);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateOwner([FromQuery]int countryId,[FromBody] OwnerDto newOwner)
        {
            if (newOwner == null)
                return BadRequest();
            var owner = _ownerRepository.GetOwners()
                .Where(o =>
                    (o.LastName.Trim().ToLower() == newOwner.LastName.Trim().ToLower())
                    && (o.FirstName.Trim().ToLower() == newOwner.FirstName.Trim().ToLower()))
                .FirstOrDefault();
            if (owner != null)
            {
                ModelState.AddModelError("", "Owner already exist");
                return StatusCode(422, ModelState);
            }
            var mappedOwner = _mapper.Map<Owner>(newOwner);
            if (!_countryRepository.IsCountryExist(countryId))
            {
                ModelState.AddModelError("", "Country does not exist");
                return BadRequest(ModelState);
            }
            mappedOwner.Country = _countryRepository.GetCountry(countryId);
            if (!_ownerRepository.CreateOwner(mappedOwner))
            {
                ModelState.AddModelError("", "Error while saving the data");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully added");
        }

        [HttpPut("{ownerId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateOwner(int ownerId, OwnerDto ownerDto)
        {
            if (ownerDto == null)
                return BadRequest(ModelState);
            if (ownerId != ownerDto.Id)
                return BadRequest(ModelState);
            if (!_ownerRepository.IsOwnerExist(ownerId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest();
            var mappedOwner = _mapper.Map<Owner>(ownerDto);
            if (!_ownerRepository.UpdateOwner(mappedOwner))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
        [HttpDelete("{ownerId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(ModelState);
            if (!_ownerRepository.IsOwnerExist(ownerId))
                return NotFound(ModelState);
            Owner OwnerToDelete = _ownerRepository.GetOwner(ownerId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_ownerRepository.DeleteOwner(OwnerToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while deleting...");
            }
            return NoContent();

        }

    }
}
