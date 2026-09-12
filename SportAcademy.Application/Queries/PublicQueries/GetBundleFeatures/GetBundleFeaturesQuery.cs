using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;

namespace SportAcademy.Application.Queries.PublicQueries.GetBundleFeatures;

// Backs both the public marketing site's bundle-builder page (anonymous) and the SuperAdmin's
// bundle-pricing editor in the platform console - same data, nothing here is sensitive to an
// anonymous visitor. See BundleFeatureDto.
public record GetBundleFeaturesQuery : IRequest<Result<List<BundleFeatureDto>>>;
