﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VS
{
    public static class ProjectExtensions
    {
        public static async Task<ProjectItem> AddFileToProjectAsync(this Project project, string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;

            if (dte.Solution.FindProjectItem(file) == null)
            {
                return project.ProjectItems.AddFromFile(file);
            }

            return null;
        }

        public static SolutionFolder AsSolutionFolder(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (SolutionFolder)project.Object;
        }

        /// <summary>Gets the root folder of any Visual Studio project.</summary>
        public static string GetRootFolder(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(project.FullName))
            {
                return null;
            }

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;
            }

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            if (File.Exists(fullPath))
            {
                return Path.GetDirectoryName(fullPath);
            }

            return null;
        }
    }
}
