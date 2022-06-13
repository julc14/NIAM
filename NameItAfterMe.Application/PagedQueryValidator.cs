using FluentValidation;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

internal class PagedQueryValidator : AbstractValidator<PagedQuery>
{
    public PagedQueryValidator()
    {
        RuleFor(x => x.PageNumber > 0);
        RuleFor(x => x.PageSize > 0);
    }
}
