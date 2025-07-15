using System.Windows.Controls;

namespace TiaAddIn
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            Browser.ObjectForScripting = new ScriptingObject();
            Browser.Navigate("http://localhost:5678/webhook-test/chat");
        }
    }
}
