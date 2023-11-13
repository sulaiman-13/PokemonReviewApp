using PokemonReviewApp.Models;

namespace PokemonReviewApp.Interfaces
{
    public interface IReviewerRepository
    {
        Reviewer GetReviewer(int reviewerId);
        ICollection<Reviewer> GetReviewers();
        ICollection<Review> GetReviewsByReviewer(int reviewerId);
        bool IsReviewerExist(int reviewerId);
        bool CreateReviewer(Reviewer reviewer);
        bool UpdateReviewer(Reviewer reviewer);
        bool DeleteReviewer(Reviewer reviewer);
        
        bool Save();
    }
}
