using System;
using Windows.UI.Popups;

namespace BLEServiceBrowser.Utils
{
    // Static cheat class to make calling a message box easier a neater in the main code.
    public static class MessageHelper
    {
        public static async void DisplayBasicMessage(string content)
        {
            await new MessageDialog(content).ShowAsync();
        }
    }
}
