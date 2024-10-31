using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace BlazorSemanticKernel.Plugins;

public class RoutingPlugin
{
    private readonly List<string> _routingDestinations = new()
    {
        "Accounting", "Legal", "Human Resources", "IT Support", "Sales", "Customer Service", "Executive Office"
    };

    [KernelFunction, Description("Route a document to a specific department")]
    public string RouteDocument(
        [Description("The department to route the document to")] string department)
    {
        if (!_routingDestinations.Contains(department))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Error: {department} is not a valid routing destination.",
                routedTo = string.Empty
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            message = $"Document successfully routed to {department}.",
            routedTo = department
        });
    }

    [KernelFunction, Description("Analyze the content of a document and suggest where it should be routed")]
    public string AnalyzeDocument(
        [Description("The content of the document to be analyzed")] string content)
    {
        return $"Document content: {content}\n\nPossible routing destinations: {string.Join(", ", _routingDestinations)}";
    }

    // Helper method to get valid destinations
    [KernelFunction, Description("Get list of valid routing destinations")]
    public string GetValidDestinations()
    {
        return JsonSerializer.Serialize(_routingDestinations);
    }
}