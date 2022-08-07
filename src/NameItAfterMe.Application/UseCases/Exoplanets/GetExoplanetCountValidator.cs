using FluentValidation;

namespace NameItAfterMe.Application.UseCases.Exoplanets
{
    public class GetExoplanetCountValidator : AbstractValidator<GetExoplanetCount>
    {
        public GetExoplanetCountValidator()
        {
            // both cant be true.
            RuleFor(x => x.ExcludeNamedExoplanets).Must((x, excludeNamedExoplanets) => !(x.ExcludeUnnamedExoplanets && excludeNamedExoplanets));
            RuleFor(x => x.ExcludeUnnamedExoplanets).Must((x, excludeUnnamedExoplanets) => !(x.ExcludeNamedExoplanets && excludeUnnamedExoplanets));
        }
    }
}