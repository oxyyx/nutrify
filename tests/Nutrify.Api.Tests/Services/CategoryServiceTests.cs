using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Entities;
using Nutrify.Api.Services;
using Nutrify.Contracts.Categories;
using TUnit.Assertions.Enums;

namespace Nutrify.Api.Tests.Services;

public class CategoryServiceTests
{
    // ---- Creation --------------------------------------------------------

    [Test]
    public async Task CreateAsync_PersistsCategory()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);

        var dto = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        await Assert.That(dto.Name).IsEqualTo("Snacks");
        await Assert.That(dto.FoodItemCount).IsEqualTo(0);

        var entity = await db.Categories.SingleAsync();
        await Assert.That(entity.Name).IsEqualTo("Snacks");
        await Assert.That(entity.UserId).IsEqualTo("user1");
    }

    [Test]
    public async Task CreateAsync_RejectsDuplicateNameForTheSameUser()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        await Assert.That(async () => await service.CreateAsync("user1", new CreateCategoryRequest("Snacks")))
            .Throws<InvalidOperationException>();
        await Assert.That(await db.Categories.CountAsync()).IsEqualTo(1);
    }

    [Test]
    public async Task CreateAsync_AllowsTheSameNameForDifferentUsers()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        // Category names are unique per user, not globally.
        var other = await service.CreateAsync("user2", new CreateCategoryRequest("Snacks"));

        await Assert.That(other.Name).IsEqualTo("Snacks");
        await Assert.That(await db.Categories.CountAsync()).IsEqualTo(2);
    }

    // ---- Update ----------------------------------------------------------

    [Test]
    public async Task UpdateAsync_RenamesCategory()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        var updated = await service.UpdateAsync(created.Id, "user1", new UpdateCategoryRequest("Treats"));

        await Assert.That(updated!.Name).IsEqualTo("Treats");
    }

    [Test]
    public async Task UpdateAsync_AllowsRenamingToItsOwnName()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        // The duplicate check must exclude the row being updated.
        var updated = await service.UpdateAsync(created.Id, "user1", new UpdateCategoryRequest("Snacks"));

        await Assert.That(updated!.Name).IsEqualTo("Snacks");
    }

    [Test]
    public async Task UpdateAsync_RejectsRenamingOntoAnExistingName()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));
        var second = await service.CreateAsync("user1", new CreateCategoryRequest("Drinks"));

        await Assert.That(async () => await service.UpdateAsync(
                second.Id, "user1", new UpdateCategoryRequest("Snacks")))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task UpdateAsync_ReturnsNullForAnotherUsersCategory()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        var updated = await service.UpdateAsync(created.Id, "user2", new UpdateCategoryRequest("Hijacked"));

        await Assert.That(updated).IsNull();
        await Assert.That((await db.Categories.SingleAsync()).Name).IsEqualTo("Snacks");
    }

    // ---- Delete ----------------------------------------------------------

    [Test]
    public async Task DeleteAsync_RemovesAnEmptyCategory()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        await Assert.That(await service.DeleteAsync(created.Id, "user1")).IsTrue();
        await Assert.That(await db.Categories.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task DeleteAsync_RejectsCategoryThatStillHasFoodItems()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));
        db.FoodItems.Add(TestDb.NewFood("user1", "Crisps", categoryId: created.Id));
        await db.SaveChangesAsync();

        await Assert.That(async () => await service.DeleteAsync(created.Id, "user1"))
            .Throws<InvalidOperationException>();
        await Assert.That(await db.Categories.AnyAsync()).IsTrue();
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseForAnotherUsersCategory()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        await Assert.That(await service.DeleteAsync(created.Id, "user2")).IsFalse();
        await Assert.That(await db.Categories.CountAsync()).IsEqualTo(1);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseForUnknownId()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);

        await Assert.That(await service.DeleteAsync(4321, "user1")).IsFalse();
    }

    // ---- Reads -----------------------------------------------------------

    [Test]
    public async Task GetAllAsync_OrdersByNameAndCountsFoodItems()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var snacks = new Category { Name = "Snacks", UserId = "user1" };
        var drinks = new Category { Name = "Drinks", UserId = "user1" };
        db.Categories.AddRange(snacks, drinks);
        await db.SaveChangesAsync();
        db.FoodItems.AddRange(
            TestDb.NewFood("user1", "Crisps", categoryId: snacks.Id),
            TestDb.NewFood("user1", "Nuts", categoryId: snacks.Id),
            TestDb.NewFood("user1", "Cola", categoryId: drinks.Id));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync("user1");

        await Assert.That(result.Select(c => c.Name).ToList())
            .IsEquivalentTo(new List<string> { "Drinks", "Snacks" }, CollectionOrdering.Matching);
        await Assert.That(result.Single(c => c.Name == "Snacks").FoodItemCount).IsEqualTo(2);
        await Assert.That(result.Single(c => c.Name == "Drinks").FoodItemCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetAllAsync_FiltersBySearch()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));
        await service.CreateAsync("user1", new CreateCategoryRequest("Drinks"));

        var result = await service.GetAllAsync("user1", search: "Snack");

        await Assert.That(result).Count().IsEqualTo(1);
        await Assert.That(result.Single().Name).IsEqualTo("Snacks");
    }

    [Test]
    public async Task GetAllAsync_DoesNotLeakOtherUsersCategories()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));
        await service.CreateAsync("user2", new CreateCategoryRequest("Snacks"));

        var result = await service.GetAllAsync("user1");

        await Assert.That(result).Count().IsEqualTo(1);
    }

    [Test]
    public async Task GetByIdAsync_IsScopedToTheCaller()
    {
        await using var db = TestDb.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync("user1", new CreateCategoryRequest("Snacks"));

        await Assert.That(await service.GetByIdAsync(created.Id, "user1")).IsNotNull();
        await Assert.That(await service.GetByIdAsync(created.Id, "user2")).IsNull();
    }
}
