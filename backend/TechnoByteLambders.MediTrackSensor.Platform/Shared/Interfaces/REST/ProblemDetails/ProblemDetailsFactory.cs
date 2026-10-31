using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Resources.Errors;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails;

public class ProblemDetailsFactory
{
    private readonly Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory _aspNetCoreProblemDetailsFactory;
    private readonly IStringLocalizer<CommonMessages> _commonLocalizer;
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;

    public ProblemDetailsFactory(
        IStringLocalizer<ErrorMessages> errorLocalizer,
        IStringLocalizer<CommonMessages> commonLocalizer,
        Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory aspNetCoreProblemDetailsFactory)
    {
        _errorLocalizer = errorLocalizer;
        _commonLocalizer = commonLocalizer;
        _aspNetCoreProblemDetailsFactory = aspNetCoreProblemDetailsFactory;
    }

    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        Enum? errorEnum,
        string detailMessage)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
            detail: detailMessage
        ) ?? new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
            Detail = detailMessage,
            Instance = controller.HttpContext.Request.Path
        };

        problemDetails.Title = errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"];
        problemDetails.Detail = detailMessage;
        problemDetails.Instance = controller.HttpContext.Request.Path;

        return controller.StatusCode(statusCode, problemDetails);
    }

    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        string titleKey,
        string detailKey,
        params object[] detailArgs)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            _commonLocalizer[titleKey],
            detail: _errorLocalizer[detailKey, detailArgs],
            instance: controller.HttpContext.Request.Path
        );
        return controller.StatusCode(statusCode, problemDetails);
    }
}
