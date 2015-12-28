using System.Windows;
using VKVoteCounter.Engine;
using VKVoteCounter.Resources;

namespace VKVoteCounter.UI
{
    /// <summary>
    /// Interaction logic for Auth.xaml
    /// </summary>
    public partial class Auth
    {
        public Auth()
        {
            InitializeComponent();
            vk.LoadCompleted += (o, args) =>
            {
                OkButton.IsEnabled = true;

            };
            vk.Navigate(string.Format("https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri=https://oauth.vk.com/blank.html&display=page&v=5.40&response_type=token", CStrings.AppId, CStrings.Scope));
        }
        
        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            char[] separators = {'=', '&'};
            var strData = vk.Source.ToString().Split(separators);
            if (strData.Length > 6)
            {
                MessageBox.Show("Warning", "Действие не подтверждено");
                return;
            }
            ProgramData.Token = strData[1];
            ProgramData.UserID = strData[5];
            Close();
        }
    }
}
