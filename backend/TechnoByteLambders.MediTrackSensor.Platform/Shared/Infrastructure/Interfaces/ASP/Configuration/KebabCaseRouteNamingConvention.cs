using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel != null
            ? new AttributeRouteModel
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            }
            : null;
    }
}
