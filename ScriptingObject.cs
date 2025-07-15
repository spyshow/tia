using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TiaAddIn
{
    [ComVisible(true)]
    public class ScriptingObject
    {
        public void SendMessageToHost(string message)
        {
            try
            {
                var command = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(message);
                if (command.Command == "CreateFunctionBlock")
                {
                    var tiaPortal = TiaPortal.GetGlobalInstance();
                    var project = tiaPortal.Projects[0];
                    var plc = (PlcSoftware)project.Devices[0].DeviceItems[1].GetService<SoftwareContainer>().Software;
                    var block = plc.BlockGroup.Blocks.Create("FunctionBlock", (string)command.Name);
                    block.Code = (string)command.Code;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing message: {ex.Message}", "Error");
            }
        }
    }
}
