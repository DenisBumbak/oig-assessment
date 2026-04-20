using FluentValidation;
using Microsoft.EntityFrameworkCore.Query;
using OIG.Application.Abstractions.Persistence;
using OIG.Application.Common.Behaviors;
using OIG.Application.Features.Users.Create;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;
using System.Linq.Expressions;

namespace OIG.Application.Tests;

public class CreateUserValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenOrganizationDoesNotExist()
    {
        var repo = new FakeOrganizationRepository();
        var validator = new CreateUserValidator(repo);

        var result = await validator.ValidateAsync(new CreateUserCommand("Test User", "test@example.com", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Organization not found.");
    }

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenOrganizationExists()
    {
        var repo = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("Root");
        await repo.AddAsync(org, CancellationToken.None);
        var validator = new CreateUserValidator(repo);

        var result = await validator.ValidateAsync(new CreateUserCommand("Test User", "test@example.com", org.Id.Value));

        Assert.True(result.IsValid);
    }
}

public class CreateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrow_WhenOrganizationDoesNotExist()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateUserHandler(users, orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateUserCommand("Test User", "test@example.com", Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateUserHandler(users, orgs, uow);

        var existing = User.Register("Existing User", "test@example.com");
        await users.AddAsync(existing, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateUserCommand("Test User", "test@example.com", null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndSave_WhenInputIsValid()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateUserHandler(users, orgs, uow);

        var response = await handler.Handle(new CreateUserCommand("Test User", "test@example.com", null), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Test User", response.Name);
        Assert.Equal("test@example.com", response.Email);
        Assert.True(response.IsActive);
        Assert.Equal(1, users.AddedCount);
        Assert.Equal(1, uow.SaveChangesCalls);
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldUseAsyncValidationRules()
    {
        var validators = new IValidator<CreateUserCommand>[]
        {
            new AsyncAlwaysFailValidator()
        };

        var behavior = new ValidationBehavior<CreateUserCommand, CreateUserResponse>(validators);

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new CreateUserCommand("Test User", "test@example.com", null),
                () => Task.FromResult(new CreateUserResponse(Guid.NewGuid(), "Test User", "test@example.com", true, null)),
                CancellationToken.None));
    }

    private sealed class AsyncAlwaysFailValidator : AbstractValidator<CreateUserCommand>
    {
        public AsyncAlwaysFailValidator()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (_, _) =>
                {
                    await Task.Delay(1);
                    return false;
                })
                .WithMessage("fail");
        }
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    public int AddedCount { get; private set; }

    public Task<User?> FindByIdAsync(UserId id, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> FindByIdWithRolesAsync(UserId id, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        => Task.FromResult(_users.FirstOrDefault(u => u.Email.Value == normalizedEmail));

    public Task AddAsync(User user, CancellationToken ct)
    {
        _users.Add(user);
        AddedCount++;
        return Task.CompletedTask;
    }

    public void Remove(User user) => _users.Remove(user);

    public IQueryable<User> Query() => new TestAsyncEnumerable<User>(_users);
}

internal sealed class FakeOrganizationRepository : IOrganizationRepository
{
    private readonly List<Organization> _orgs = [];

    public Task<Organization?> FindByIdAsync(OrganizationId id, CancellationToken ct)
        => Task.FromResult(_orgs.FirstOrDefault(o => o.Id == id));

    public Task AddAsync(Organization org, CancellationToken ct)
    {
        _orgs.Add(org);
        return Task.CompletedTask;
    }

    public void Remove(Organization org) => _orgs.Remove(org);

    public IQueryable<Organization> Query() => new TestAsyncEnumerable<Organization>(_orgs);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = [];

    public Task<Role?> FindByIdAsync(RoleId id, CancellationToken ct)
        => Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));

    public Task AddAsync(Role role, CancellationToken ct)
    {
        _roles.Add(role);
        return Task.CompletedTask;
    }

    public void Remove(Role role) => _roles.Remove(role);

    public IQueryable<Role> Query() => new TestAsyncEnumerable<Role>(_roles);
}

internal sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => (TResult)(object)Task.FromResult(Execute<object>(expression));
}

internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
        => ValueTask.FromResult(inner.MoveNext());
}
