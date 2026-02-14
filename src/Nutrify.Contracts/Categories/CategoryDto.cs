namespace Nutrify.Contracts.Categories;

public record CategoryDto(
    int Id,
    string Name,
    int FoodItemCount
);
