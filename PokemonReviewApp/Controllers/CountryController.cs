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
    public class CountryController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public CountryController(ICountryRepository countryRepository, IMapper mapper)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetCountries()
        {
            var countries = _mapper.Map<List<CountryDto>>(_countryRepository.GetCountries());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(countries);
        }



        [HttpGet("{countryId}")]
        [ProducesResponseType(200, Type = typeof(CountryDto))]
        [ProducesResponseType(400)]
        public IActionResult GetCountry(int countryId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_countryRepository.IsCountryExist(countryId))
                return NotFound();
            var Country = _mapper.Map<CountryDto>(_countryRepository.GetCountry(countryId));
            return Ok(Country);
        }
        [HttpGet("owners/{ownerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(CountryDto))]

        public IActionResult GetCountryByOwnerId(int ownerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var country = _mapper.Map<CountryDto>(_countryRepository.GetCountryByOwner(ownerId));
            return Ok(country);
        }


        [HttpGet("{countryId}/owners")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<OwnerDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetOwnersFromACountry(int countryId)
        {
            if (!_countryRepository.IsCountryExist(countryId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var owners = _mapper.Map<List<OwnerDto>>(_countryRepository.GetOwnersFromACountry(countryId));
            return Ok(owners);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCountry(CountryDto newCountry)
        {
            if (newCountry == null)
                return BadRequest();
            var country = _countryRepository.GetCountries()
                .Where(c => c.Name.Trim().ToLower() == newCountry.Name.TrimEnd().ToLower()).FirstOrDefault();
            if (country != null)
            {
                ModelState.AddModelError("", "Country is already exist");
                return StatusCode(422, ModelState);
            }
            var countryMapped = _mapper.Map<Country>(newCountry);
            if (!_countryRepository.CreateCountry(countryMapped))
            {
                ModelState.AddModelError("", "Something went wrong while saving the data");
                return StatusCode(500, ModelState);

            }
            return Ok("Successfully created");
        }
        [HttpPut("{countryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCountry(int countryId, CountryDto countryDto)
        {
            if (countryDto == null)
                return BadRequest(ModelState);
            if (countryId != countryDto.Id)
                return BadRequest(ModelState);
            if (!_countryRepository.IsCountryExist(countryId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest();
            var mappedCountry = _mapper.Map<Country>(countryDto);
            if(!_countryRepository.UpdateCountry(mappedCountry))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
        [HttpDelete("{countryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCountry(int countryId)
        {
            if (countryId <= 0)
                return BadRequest(ModelState);
            if (!_countryRepository.IsCountryExist(countryId))
                return NotFound(ModelState);
            Country CountryToDelete = _countryRepository.GetCountry(countryId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_countryRepository.DeleteCountry(CountryToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while deleting...");
            }
            return NoContent();

        }

    }
}
