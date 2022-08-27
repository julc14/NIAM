using FluentValidation;

namespace NameItAfterMe.Application.UseCases.Exoplanets.GetUnnamedExoplanets;

public class GetUnnamedExoplanetsValidator : AbstractValidator<GetUnnamedExoplanets>
{
    public GetUnnamedExoplanetsValidator()
    {
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThan(0);
    }
}