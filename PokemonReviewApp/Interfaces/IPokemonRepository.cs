using PokemonReviewApp.Models;

namespace PokemonReviewApp.Interfaces
{
    public interface IPokemonRepository
    {
         ICollection<Pokemon> GetPokemons();
         Pokemon GetPokemon(int id);
         Pokemon GetPokemon(string name);
         Decimal GetPokemonRating(int id);
         bool IsPokemonExist(int id);
        bool CreatePokemon(int ownerId, int categoryId, Pokemon pokemon);

        bool UpdatePokemon( Pokemon pokemon);
        bool DeletePokemon( Pokemon pokemon);

        bool Save();

    }
}
