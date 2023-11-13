using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public PokemonController(IPokemonRepository pokemonRepository, IOwnerRepository ownerRepository, IReviewRepository reviewRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _ownerRepository = ownerRepository;
            _reviewRepository = reviewRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _pokemons = _pokemonRepository.GetPokemons();

            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemons);


            return Ok(pokemons);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int id)
        {
            if (!_pokemonRepository.IsPokemonExist(id))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(id));

            return Ok(pokemon);
        }


        [HttpGet("{id}/rating")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonRating(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_pokemonRepository.IsPokemonExist(id))
                return NotFound();
            var rating = _pokemonRepository.GetPokemonRating(id);

            return Ok(rating);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int categoryId, [FromBody] PokemonDto newPokemon)
        {
            if (newPokemon == null)
                return BadRequest();
            var pokemon = _pokemonRepository.GetPokemons()
                .Where(p => p.Name.Trim().ToLower() == newPokemon.Name.Trim().ToLower())
                .FirstOrDefault();
            if (pokemon != null)
            {
                ModelState.AddModelError("", "pokemon already exist");
                return StatusCode(422, ModelState);
            }
            if (!_ownerRepository.IsOwnerExist(ownerId))
            {
                ModelState.AddModelError("", "Owner does not  exist");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.IsCategoryExist(categoryId))
            {
                ModelState.AddModelError("", "Category does not exist");
                return BadRequest(ModelState);
            }

            var mappedPokemon = _mapper.Map<Pokemon>(newPokemon);
            if (!_pokemonRepository.CreatePokemon(ownerId, categoryId, mappedPokemon))
            {
                ModelState.AddModelError("", "Error while saving the data");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully added");
        }


        [HttpPut("{pokemonId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePokemon(int pokemonId, [FromBody] PokemonDto pokemonDto)
        {
            if (pokemonDto == null)
                return BadRequest(ModelState);

            if (pokemonId != pokemonDto.Id)
                return BadRequest(ModelState);

            if (!_pokemonRepository.IsPokemonExist(pokemonId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var pokemonMap = _mapper.Map<Pokemon>(pokemonDto);

            if (!_pokemonRepository.UpdatePokemon(pokemonMap))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{pokemonId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeletePokemon(int pokemonId)
        {
            if (pokemonId <= 0)
                return BadRequest(ModelState);
            if (!_pokemonRepository.IsPokemonExist(pokemonId))
                return NotFound(ModelState);
            List<Review> reviewsToDelete = _reviewRepository.GetReviewsOfAPokemon(pokemonId).ToList();
            Pokemon PokemonToDelete = _pokemonRepository.GetPokemon(pokemonId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_reviewRepository.DeleteReviews(reviewsToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while deleting reviews...");
            }
            if (!_pokemonRepository.DeletePokemon(PokemonToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong while deleting pokemon {PokemonToDelete.Name}...");
            }
            return NoContent();

        }

    }
}
