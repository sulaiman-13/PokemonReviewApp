using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repositories;
using System.Data.SqlTypes;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        public IActionResult GetCategories()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var categories = _mapper.Map<List<CategoryDto>>(_categoryRepository.GetCategories());

            return Ok(categories);
        }
        [HttpGet("{categoryId}")]
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        [ProducesResponseType(400)]
        public IActionResult GetCategory([FromRoute] int categoryId)
        {
            if (!_categoryRepository.IsCategoryExist(categoryId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var category = _mapper.Map<CategoryDto>(_categoryRepository.GetCategory(categoryId));
            return Ok(category);
        }
        [HttpGet("{categoryId}/pokemons")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PokemonDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonsByCategory(int categoryId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_categoryRepository.IsCategoryExist(categoryId))
                return NotFound();
            var pokemons = _mapper.Map<List<PokemonDto>>(_categoryRepository.GetPokemonsByCategory(categoryId));

            return Ok(pokemons);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCategory([FromBody] CategoryDto newCategory)
        {
            if (newCategory == null)
                return BadRequest(ModelState);
            var category = _categoryRepository.GetCategories()
                .Where(c => c.Name.Trim().ToLower() == newCategory.Name.TrimEnd().ToLower()).FirstOrDefault();
            if (category != null)
            {
                ModelState.AddModelError("", "category is already exist");
                return StatusCode(422, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var categoryMap = _mapper.Map<Category>(newCategory);
            if (!_categoryRepository.CreateCategory(categoryMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving the data");
                return StatusCode(500, ModelState);

            }
            return Ok("Successfully created");
        }
        [HttpPut("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryDto categoryDto)
        {

            if (categoryDto == null)
                return BadRequest(ModelState);
            if (categoryDto.Id != categoryId)
                return BadRequest(ModelState);
            if (!_categoryRepository.IsCategoryExist(categoryId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest();
            var category = _mapper.Map<Category>(categoryDto);
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCategory(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest(ModelState);
            if (!_categoryRepository.IsCategoryExist(categoryId))
                return NotFound(ModelState);
           Category categoryToDelete = _categoryRepository.GetCategory(categoryId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_categoryRepository.DeleteCategory(categoryToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while deleting...");
            }
            return NoContent();

        }
    }
}
