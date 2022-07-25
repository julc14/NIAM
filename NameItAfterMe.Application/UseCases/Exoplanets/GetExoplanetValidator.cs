﻿using FluentValidation;

namespace NameItAfterMe.Application.UseCases.Exoplanets;

public class GetExoplanetValidator : AbstractValidator<GetExoplanets>
{
    public GetExoplanetValidator()
    {
        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThan(0);
    }
}