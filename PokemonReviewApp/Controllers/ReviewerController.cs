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
    public class ReviewerController : ControllerBase
    {
        private readonly IReviewerRepository _reviewerRepository;
        private readonly IMapper _mapper;
        private IReviewRepository _reviewRepository;

        public ReviewerController(IReviewerRepository reviewerRepository, IMapper mapper, IReviewRepository reviewRepository)
        {
            _reviewerRepository = reviewerRepository;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
        }



        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewerDto>))]
        public IActionResult GetReviewers()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var Reviewers = _mapper.Map<List<ReviewerDto>>(_reviewerRepository.GetReviewers());
            
            return Ok(Reviewers);
        }


        [HttpGet("{reviewerId}")]
        [ProducesResponseType(200, Type = typeof(ReviewerDto))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewer([FromRoute] int reviewerId)
        {
            if (!_reviewerRepository.IsReviewerExist(reviewerId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reviewer = _mapper.Map<ReviewerDto>(_reviewerRepository.GetReviewer(reviewerId));
            return Ok(reviewer);
        }


        [HttpGet("{reviewerId}/reviews")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetReviewsByReviewer(int reviewerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_reviewerRepository.IsReviewerExist(reviewerId))
                return NotFound();
            var reviews = _mapper.Map<List<ReviewDto>>(_reviewerRepository.GetReviewsByReviewer(reviewerId));

            return Ok(reviews);
        }
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateCategory([FromBody] ReviewerDto newReviewer)
        {
            if (newReviewer == null)
                return BadRequest(ModelState);
            var reviewer = _reviewerRepository.GetReviewers()
                .Where(c => c.FirstName.Trim().ToLower() == newReviewer.FirstName.TrimEnd().ToLower()
                && c.LastName.Trim().ToLower() == newReviewer.LastName.TrimEnd().ToLower()).FirstOrDefault();
            if (reviewer != null)
            {
                ModelState.AddModelError("", "Reviewer is already exist");
                return StatusCode(422, ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var mappedReviewer = _mapper.Map<Reviewer>(newReviewer);
            if (!_reviewerRepository.CreateReviewer(mappedReviewer))
            {
                ModelState.AddModelError("", "Something went wrong while saving the data");
                return StatusCode(500, ModelState);

            }
            return Ok("Successfully created");
        }
        [HttpPut("{reviewerId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateReviewer(int reviewerId, [FromBody] ReviewDto reviewDto)
        {

            if (reviewDto == null)
                return BadRequest(ModelState);
            if (reviewDto.Id != reviewerId)
                return BadRequest(ModelState);
            if (!_reviewerRepository.IsReviewerExist(reviewerId))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest();
            var reviewer = _mapper.Map<Reviewer>(reviewDto);
            if (!_reviewerRepository.UpdateReviewer(reviewer))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating the data");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{reviewerId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteReviewer(int reviewerId)
        {
            if (!_reviewerRepository.IsReviewerExist(reviewerId))
            {
                return NotFound();
            }

            var reviewerToDelete = _reviewerRepository.GetReviewer(reviewerId);
            List<Review> reviewsToDelete = _reviewerRepository.GetReviewsByReviewer(reviewerId).ToList();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.DeleteReviews(reviewsToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting reviews");
            }
            if (!_reviewerRepository.DeleteReviewer(reviewerToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting reviewer");
            }

            return NoContent();
        }

    }
}
