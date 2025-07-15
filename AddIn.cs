using System;
using System.Windows.Forms;
using Siemens.Engineering;
using Siemens.Engineering.AddIn;
using Siemens.Engineering.AddIn.Menu;

namespace TiaAddIn
{
    public class AddIn : MenuAddIn
    {
        private TiaPortal _tiaPortal;

        public AddIn(TiaPortal tiaPortal) : base("My Add-In")
        {
            _tiaPortal = tiaPortal;
        }

        protected override void OnCreateMenu(MenuSelection service)
        {
            service.Add("Root", "Get Project Info", OnGetProjectInfo, OnCanGetProjectInfo);
        }

        private void OnGetProjectInfo(MenuSelection selection)
        {
            var project = _tiaPortal.Projects[0];
            var projectName = project.Name;
            var projectPath = project.Path;

            var devices = project.Devices;
            var projectData = new
            {
                ProjectName = projectName,
                ProjectPath = projectPath,
                Devices = devices.Select(d => new
                {
                    DeviceName = d.Name,
                    Blocks = GetSoftwareBlocks(d)
                }).ToList()
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(projectData, Newtonsoft.Json.Formatting.Indented);

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = client.PostAsync("http://localhost:5678/webhook-test/start", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var window = new System.Windows.Window
                        {
                            Title = "Chat",
                            Content = new ChatView(),
                            Width = 800,
                            Height = 600
                        };
                        window.Show();
                    }
                    else
                    {
                        MessageBox.Show($"Error sending data to backend: {response.StatusCode}", "Error");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data to backend: {ex.Message}", "Error");
            }
        }

        private object GetSoftwareBlocks(Device device)
        {
            var softwareContainer = device.DeviceItems[0].GetService<SoftwareContainer>();
            if (softwareContainer == null)
            {
                return null;
            }

            if (softwareContainer.Software is PlcSoftware plcSoftware)
            {
                return plcSoftware.BlockGroup.Blocks.Select(b => new { Name = b.Name, Type = b.GetType().Name }).ToList();
            }
            else if (softwareContainer.Software is HmiTarget hmiTarget)
            {
                return new
                {
                    Screens = hmiTarget.ScreenService.Screens.Select(s => new { Name = s.Name }).ToList(),
                    Tags = hmiTarget.TagFolder.Tags.Select(t => new { Name = t.Name }).ToList()
                };
            }

            return null;
        }

        private bool OnCanGetProjectInfo(MenuSelection selection)
        {
            return _tiaPortal.Projects.Count > 0;
        }
    }
}
