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
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;

        public ReviewController(IReviewRepository ReviewRepository,IPokemonRepository pokemonRepository,IReviewerRepository reviewerRepository, IMapper mapper)
        {
            _reviewRepository = ReviewRepository;
            _pokemonRepository = pokemonRepository;
            _reviewerRepository = reviewerRepository;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        public IActionResult GetReviews()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviews());
            
            return Ok(reviews);
        }
        [HttpGet("{reviewId}")]
        [ProducesResponseType(200, Type = typeof(ReviewDto))]
        [ProducesResponseType(400)]
        public IActionResult GetReview([FromRoute] int reviewId)
        {
            if (!_reviewRepository.IsReviewExist(reviewId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var review = _mapper.Map<ReviewDto>(_reviewRepository.GetReview(reviewId));
            return Ok(review);
        }
        [HttpGet("pokemon/{pokemonId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsOfAPokemon(int pokemonId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewRepository.GetReviewsOfAPokemon(pokemonId));

            return Ok(reviews);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateReview([FromQuery] int pokemonId, [FromQuery] int reviewerId, [FromBody] ReviewDto newReview)
        {
            if (newReview == null)
                return BadRequest();
            var review = _reviewRepository.GetReviews()
                .Where(p => p.Title.Trim().ToLower() == newReview.Title.Trim().ToLower())
                .FirstOrDefault();
            if (review != null)
            {
                ModelState.AddModelError("", "Review already exist");
                return StatusCode(422, ModelState);
            }
            if (!_pokemonRepository.IsPokemonExist(pokemonId))
            {
                ModelState.AddModelError("", "Pokemon does not  exist");
                return BadRequest(ModelState);
            }
            if (!_reviewerRepository.IsReviewerExist(reviewerId))
            {
                ModelState.AddModelError("", "Reviewer does not exist");
                return BadRequest(ModelState);
            }

            var mappedReview = _mapper.Map<Review>(newReview);
            if (!_reviewRepository.CreateReview(pokemonId, reviewerId, mappedReview))
            {
                ModelState.AddModelError("", "Error while saving the data");
                return StatusCode(500, ModelState);
            }
            return Ok("Successfully added");
        }
        [HttpPut("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDto reviewDto)
        {

            if (reviewDto == null)
                return BadRequest(ModelState);
            if (reviewDto.Id != reviewId)
                return BadRequest(ModelState);
            if (!_reviewRepository.IsReviewExist(reviewId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest();
            var review = _mapper.Map<Review>(reviewDto);
            if (!_reviewRepository.UpdateReview(review))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{reviewId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReview(int reviewId)
        {
            if (!_reviewRepository.IsReviewExist(reviewId))
            {
                return NotFound();
            }

            var reviewToDelete = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.DeleteReview(reviewToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting...");
            }

            return NoContent();
        }

    }
}
