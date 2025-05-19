using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using PlasticGui;
using PlasticGui.WebApi.Responses;
using PlasticGui.WorkspaceWindow.Home;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal class OrganizationPanel : VisualElement
    {
        internal OrganizationPanel(
            CloudEditionWelcomeWindow parentWindow,
            OrganizationsResponse organizationResponse,
            Action<string> organizationJoinedAction,
            string title)
        {
            mParentWindow = parentWindow;
            mOrganizationResponse = organizationResponse;
            mOnOrganizationJoined = organizationJoinedAction;

            InitializeLayoutAndStyles();

            BuildComponents(title);

            OnEditorActivated();
        }

        internal void Dispose()
        {
            mParentWindow.CancelJoinOrganization();

            if (mJoinSingleOrganizationButton != null)
                mJoinSingleOrganizationButton.clicked -= JoinOrganizationButton_clicked;

            if (mJoinMultipleOrganizationsButton != null)
                mJoinMultipleOrganizationsButton.clicked -= JoinOrganizationButton_clicked;

            if (mOpenUnityDashboardButton != null)
                mOpenUnityDashboardButton.clicked -= OpenUnityDashboardButton_clicked;
        }

        void OnEditorActivated()
        {
            if (mOrganizationResponse == null || mOrganizationResponse.CloudServers == null)
            {
                mProgressControls.ShowError("Could not find cloud organizations");
                return;
            }

            if (mOrganizationResponse.Error != null)
            {
                mProgressControls.ShowError(mOrganizationResponse.Error.Message);
                return;
            }

            // This is crucial to initialize the internal cache of slugs in the plastic library
            OrganizationsInformation.UpdateOrganizationSlugs(mOrganizationResponse);

            List<OrganizationInfo> organizationsInfo = OrganizationsInformation.FromServersOrdered(mOrganizationResponse.CloudServers);

            ProcessOrganizations(organizationsInfo);
        }

        void ProcessOrganizations(List<OrganizationInfo> organizations)
        {
            this.Query<VisualElement>("noOrganization").Collapse();
            this.Query<VisualElement>("joinSingleOrganization").Collapse();
            this.Query<VisualElement>("joinMultipleOrganizations").Collapse();

            if (organizations.Count == 0)
            {
                BuildNoOrganizationSection();

                mOpenUnityDashboardButton = this.Q<Button>("openUnityDashboardButton");
                mOpenUnityDashboardButton.clicked += OpenUnityDashboardButton_clicked;

                return;
            }

            if (organizations.Count == 1)
            {
                BuildSingleOrganizationSection(organizations.First());

                mJoinSingleOrganizationButton = this.Q<Button>("joinSingleOrganizationButton");
                mJoinSingleOrganizationButton.clicked += JoinOrganizationButton_clicked;

                return;
            }

            BuildMultipleOrganizationsSection(organizations);

            mJoinMultipleOrganizationsButton = this.Q<Button>("joinMultipleOrganizationsButton");
            mJoinMultipleOrganizationsButton.clicked += JoinOrganizationButton_clicked;
            mOrganizationToJoin = organizations.First().Server;
        }

        void InitializeLayoutAndStyles()
        {
            this.LoadLayout(typeof(OrganizationPanel).Name);
            this.LoadStyle(typeof(OrganizationPanel).Name);
        }

        void JoinOrganizationButton_clicked()
        {
            mOnOrganizationJoined(mOrganizationToJoin);

            // TODO: Closing the window for now. Need to connect this event to the main on boarding workflow.
            mParentWindow.Close();
        }

        void OpenUnityDashboardButton_clicked()
        {
            Application.OpenURL(UnityUrl.UnityDashboard.Plastic.Get());
        }

        void BuildComponents(string title)
        {
            mParentWindow.titleContent = new UnityEngine.GUIContent(title);

            mProgressControls = new ProgressControlsForDialogs(null);

            mProgressContainer = this.Q<VisualElement>("progressContainer");
            mProgressContainer.Add((VisualElement)mProgressControls);
        }

        void BuildSingleOrganizationSection(OrganizationInfo organizationInfo)
        {
            this.SetControlText<Label>("confirmationMessage",
                PlasticLocalization.Name.JoinOrganizationTitle);

            mOrganizationToJoin = organizationInfo.Server;

            this.Query<VisualElement>("joinSingleOrganization").Show();

            this.SetControlText<Label>("joinSingleOrganizationLabel",
                PlasticLocalization.Name.YouBelongToOrganization,  organizationInfo.DisplayName);

            this.SetControlText<Button>("joinSingleOrganizationButton",
                PlasticLocalization.Name.JoinButton);
        }

        void BuildMultipleOrganizationsSection(List<OrganizationInfo> organizations)
        {
            organizations.Sort((x, y) =>
                string.Compare(x.DisplayName, y.DisplayName, StringComparison.CurrentCulture));

            this.SetControlText<Label>("confirmationMessage",
                PlasticLocalization.Name.JoinOrganizationTitle);

            this.Query<VisualElement>("joinMultipleOrganizations").Show();

            this.SetControlText<Label>("joinMultipleOrganizationsLabel",
                PlasticLocalization.Name.YouBelongToSeveralOrganizations);

            VisualElement organizationDropdown = this.Query<VisualElement>("organizationDropdown");
            ToolbarMenu toolbarMenu = new ToolbarMenu
            {
                text = organizations.First().DisplayName,
            };

            foreach (OrganizationInfo organization in organizations)
            {
                string organizationDisplayName = organization.DisplayName;

                toolbarMenu.menu.AppendAction(organizationDisplayName, x => 
                {
                    toolbarMenu.text = organizationDisplayName;
                    mOrganizationToJoin = organization.Server;
                }, DropdownMenuAction.AlwaysEnabled);
                organizationDropdown.Add(toolbarMenu);
            }

            this.SetControlText<Button>("joinMultipleOrganizationsButton",
                PlasticLocalization.Name.JoinButton);
        }

        void BuildNoOrganizationSection()
        {
            this.SetControlText<Label>("confirmationMessage",
                PlasticLocalization.Name.CreateOrganizationTitle);

            this.Query<VisualElement>("noOrganization").Show();

            this.SetControlImage("iconUnity",
                Images.Name.ButtonSsoSignInUnity);

            this.SetControlText<Label>("noOrganizationLabel",
                PlasticLocalization.Name.ClickButtonBelowToCreateOrg);

            this.SetControlText<Button>("openUnityDashboardButton",
                PlasticLocalization.Name.MainSidebarOpenUnityDashboardItem);
        }

        string mOrganizationToJoin = "";

        Button mJoinSingleOrganizationButton;
        Button mJoinMultipleOrganizationsButton;
        Button mOpenUnityDashboardButton;
        VisualElement mProgressContainer;

        IProgressControls mProgressControls;

        readonly CloudEditionWelcomeWindow mParentWindow;
        readonly OrganizationsResponse mOrganizationResponse;
        private readonly Action<string> mOnOrganizationJoined;
    }
}