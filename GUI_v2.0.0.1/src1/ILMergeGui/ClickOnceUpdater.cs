namespace ILMergeGui
{
    using System;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Windows.Forms;

    public class ClickOnceUpdater
    {
        internal static void InstallUpdateSyncWithInfo(string Url)
        {
            string caption = $"{Mainform.AppTitle} - {"Updater"}";
            UpdateCheckInfo info = null;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment currentDeployment = ApplicationDeployment.CurrentDeployment;
                try
                {
                    info = currentDeployment.CheckForDetailedUpdate();
                }
                catch (DeploymentDownloadException exception)
                {
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + exception.Message, caption);
                    return;
                }
                catch (InvalidDeploymentException exception2)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + exception2.Message, caption);
                    return;
                }
                catch (InvalidOperationException exception3)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + exception3.Message, caption);
                    return;
                }
                if (info.UpdateAvailable)
                {
                    bool flag = true;
                    if (!info.IsUpdateRequired)
                    {
                        DialogResult result = MessageBox.Show("An update is available. Would you like to update the application now?", caption, MessageBoxButtons.OKCancel);
                        if (DialogResult.OK != result)
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("This application has detected a mandatory update from your current version to version " + info.MinimumRequiredVersion.ToString() + ". The application will now install the update and restart.", caption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    if (!flag)
                    {
                        return;
                    }
                    try
                    {
                        currentDeployment.Update();
                        MessageBox.Show("The application has been upgraded, and will now restart.", caption);
                        Application.Restart();
                        return;
                    }
                    catch (DeploymentDownloadException exception4)
                    {
                        MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + exception4, caption);
                        return;
                    }
                }
                MessageBox.Show("There is currently no update available.", caption);
            }
            else if (MessageBox.Show($"This automated check is only available for Click-Once Installers

Visit the {caption} website?", caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                VisitWebsite(Url);
            }
        }

        internal static void VisitWebsite(string Url)
        {
            Process.Start(Url);
        }
    }
}

