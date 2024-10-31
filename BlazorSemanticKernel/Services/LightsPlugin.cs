using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace BlazorSemanticKernel.Services
{
    public class LightsPlugin
    {
        private readonly List<LightModel> lights = new()
        {
            new LightModel { Id = 1, Name = "Living Room", IsOn = false, Brightness = 0, Color = "White" },
            new LightModel { Id = 2, Name = "Kitchen", IsOn = false, Brightness = 0, Color = "White" },
            new LightModel { Id = 3, Name = "Bedroom", IsOn = false, Brightness = 0, Color = "White" }
        };

        [KernelFunction, Description("Get a list of all lights and their current state")]
        public List<LightModel> GetLights() => lights;

        [KernelFunction, Description("Turn a specific light on or off")]
        public LightModel? ToggleLight(int id, bool isOn)
        {
            var light = lights.FirstOrDefault(l => l.Id == id);
            if (light != null)
            {
                light.IsOn = isOn;
                light.Brightness = isOn ? 100 : 0;
            }
            return light;
        }

        [KernelFunction, Description("Set the brightness of a specific light")]
        public LightModel? SetBrightness(int id, int brightness)
        {
            var light = lights.FirstOrDefault(l => l.Id == id);
            if (light != null)
            {
                light.Brightness = Math.Clamp(brightness, 0, 100);
                light.IsOn = light.Brightness > 0;
            }
            return light;
        }

        [KernelFunction, Description("Change the color of a specific light")]
        public LightModel? ChangeColor(int id, string color)
        {
            var light = lights.FirstOrDefault(l => l.Id == id);
            if (light != null)
            {
                light.Color = color;
            }
            return light;
        }
    }

    public class LightModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public int Brightness { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}