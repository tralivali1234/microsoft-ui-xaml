﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Windows.UI.Xaml.Tests.MUXControls.InteractionTests.Common;
using Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

#if BUILD_WINDOWS
using System.Windows.Automation;
using MS.Internal.Mita.Foundation;
using MS.Internal.Mita.Foundation.Controls;
using MS.Internal.Mita.Foundation.Patterns;
using MS.Internal.Mita.Foundation.Waiters;
#else
using Microsoft.Windows.Apps.Test.Automation;
using Microsoft.Windows.Apps.Test.Foundation;
using Microsoft.Windows.Apps.Test.Foundation.Controls;
using Microsoft.Windows.Apps.Test.Foundation.Patterns;
using Microsoft.Windows.Apps.Test.Foundation.Waiters;
#endif

namespace Windows.UI.Xaml.Tests.MUXControls.InteractionTests.Infra
{
    public enum TestType { MuxControls, Nuget, NugetCX, WPFXAMLIsland };

    // This is marked as a test class to make sure our AssemblyInitialize and AssemblyCleanup
    // fixtures get executed.  It won't actually host any tests.
    [TestClass]
    public class TestEnvironment
    {
        private const string _testAppPackageName = "MUXControlsTestApp";

#if USING_TAEF
        private const string _testAppName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_8wekyb3d8bbwe!taef.executionengine.universal.App";
#else
        private const string _testAppName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_8wekyb3d8bbwe!App";
#endif

        private const string _wpfXamlIslandPackageName = "MUXControlsTestAppWPFPackage";
        private const string _wfpXamlIslandAppName = "MUXControlsTestAppWPFPackage_8wekyb3d8bbwe!App";

        public const string _nugetTestAppPackageName = "NugetPackageTestApp";
        private const string _nugetTestAppName = "NugetPackageTestApp_8wekyb3d8bbwe!App";

        public const string _nugetTestAppCXPackageName = "NugetPackageTestAppCX";
        private const string _nugetTestAppCXName = "NugetPackageTestAppCX_8wekyb3d8bbwe!App";

#if BUILD_WINDOWS
        private const string _testAppPackageFullName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_1.0.0.0_neutral__8wekyb3d8bbwe";
        private const string _nugetTestAppPackageFullName = "NugetPackageTestApp_1.0.0.0_neutral__8wekyb3d8bbwe";
        private const string _nugetTestAppCXPackageFullName = "NugetPackageTestAppCX_1.0.0.0_neutral__8wekyb3d8bbwe";
        private const string _wpfXamlIslandPackageFullName = "MUXControlsTestAppWPFPackage_1.0.0.0_neutral__8wekyb3d8bbwe";
#elif ARM
        private const string _testAppPackageFullName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_1.0.0.0_arm__8wekyb3d8bbwe";
        private const string _nugetTestAppPackageFullName = "NugetPackageTestApp_1.0.0.0_arm__8wekyb3d8bbwe";
        private const string _nugetTestAppCXPackageFullName = "NugetPackageTestAppCX_1.0.0.0_arm__8wekyb3d8bbwe";
        private const string _wpfXamlIslandPackageFullName = "MUXControlsTestAppWPFPackage_1.0.0.0_arm__8wekyb3d8bbwe";
#elif X64
        private const string _testAppPackageFullName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_1.0.0.0_x64__8wekyb3d8bbwe";
        private const string _nugetTestAppPackageFullName = "NugetPackageTestApp_1.0.0.0_x64__8wekyb3d8bbwe";
        private const string _nugetTestAppCXPackageFullName = "NugetPackageTestAppCX_1.0.0.0_x64__8wekyb3d8bbwe";
        private const string _wpfXamlIslandPackageFullName = "MUXControlsTestAppWPFPackage_1.0.0.0_x64__8wekyb3d8bbwe";
#else
        private const string _testAppPackageFullName = "407b1cc5-f51e-4bfa-b5d2-04afa83fe380_1.0.0.0_x86__8wekyb3d8bbwe";
        private const string _nugetTestAppPackageFullName = "NugetPackageTestApp_1.0.0.0_x86__8wekyb3d8bbwe";
        private const string _nugetTestAppCXPackageFullName = "NugetPackageTestAppCX_1.0.0.0_x86__8wekyb3d8bbwe";
        private const string _wpfXamlIslandPackageFullName = "MUXControlsTestAppWPFPackage_1.0.0.0_x86__8wekyb3d8bbwe";
#endif

        public static TestContext TestContext { get; private set; }

        public static bool IsLogVerbose { get; private set; }

        public static bool IsLogSuperVerbose { get; private set; }

        public static Application Application { get; private set; }

        public static bool ShouldRestartApplication { get; set; }

        public static void LogVerbose(string format, params object[] args)
        {
            if (IsLogVerbose)
            {
                Log.Comment(format, args);
            }
        }

        public static void LogSuperVerbose(string format, params object[] args)
        {
            if (IsLogSuperVerbose)
            {
                Log.Comment(format, args);
            }
        }

        [AssemblyInitialize]
#if BUILD_WINDOWS
        [TestProperty("CoreClrProfile", "TestNetv2.1")]
#else
        [TestProperty("CoreClrProfile", ".NETCoreApp2.1")]
#endif
        [TestProperty("RunFixtureAs:Assembly", "ElevatedUserOrSystem")]
        [TestProperty("Hosting:Mode", "UAP")]
        // Default value for test metadata used to group tests 
        [TestProperty("MUXControlsTestSuite", "SuiteA")]
        // Default value for tests is to not run on phone. Test Classes or Test Methods can override
        [TestProperty("MUXControlsTestEnabledForPhone", "False")]
        public static void AssemblyInitialize(TestContext testContext)
        {
#if USING_TAEF
            Log.Comment("TestContext.TestDeploymentDir    = {0}", testContext.TestDeploymentDir);
            Log.Comment("TestContext.TestDir              = {0}", testContext.TestDir);
            Log.Comment("TestContext.TestLogsDir          = {0}", testContext.TestLogsDir);
            Log.Comment("TestContext.TestResultsDirectory = {0}", testContext.TestResultsDirectory);
            Log.Comment("TestContext.DeploymentDirectory  = {0}", testContext.DeploymentDirectory);

            // Enable side-loading
            Log.Comment("Enable side loading apps");
            TestAppInstallHelper.EnableSideloadingApps();

            // Install the test app certificate if we're deploying the MUXControlsTestApp from the NuGet package.
            // If this is the MUXControlsTestApp from the OS repo, then it'll have been signed with a test cert
            // that doesn't need installation.
#if !BUILD_WINDOWS
            Log.Comment("Installing the certificate for the test app");
            TestAppInstallHelper.InstallAppxCert(testContext.TestDeploymentDir, "MUXControlsTestApp");
#endif
#endif
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            AssemblyCleanupWorker(TestType.MuxControls);
        }

        public static void AssemblyCleanupWorker(TestType testType)
        {
            // This executed in a different context from the tests, so it doesn't have a reference
            // to Application object created by them, so just create a local one (configured to not launch the
            // app) so that we can close it.
            var app = CreateApplication(testType);
            app.Close();

            Log.Comment("Killing processes we might have inadvertently started...");
            var killList = new string[] { "microsoft.photos", "HxAccounts", "HxCalendarAppImm", "HxOutlook", "HxTsr" };
            foreach (var process in Process.GetProcesses().Where(p => killList.Contains(p.ProcessName.ToLower())))
            {
                Log.Comment("Killing process '{0}' ({1}).", process.ProcessName, process.Id);
                process.Kill();
                process.WaitForExit();
            }
        }

        private static Application CreateApplication(TestType type)
        {
            if (type == TestType.Nuget) return new Application(_nugetTestAppPackageName, _nugetTestAppPackageFullName, _nugetTestAppName);
            if (type == TestType.NugetCX) return new Application(_nugetTestAppCXPackageName, _nugetTestAppCXPackageFullName, _nugetTestAppCXName);
            if (type == TestType.WPFXAMLIsland) return new Application(_wpfXamlIslandPackageName, _wpfXamlIslandPackageFullName, _wfpXamlIslandAppName, false /* isUWPApp */);
            return new Application(_testAppPackageName, _testAppPackageFullName, _testAppName);
        }

        // Tests classes call this from their ClassInitialize methods to init our Application instance
        // and launching the application if necessary.
        public static void Initialize(TestContext testContext, TestType type = TestType.MuxControls)
        {
            TestContext = testContext;

#if USING_TAEF
            IsLogVerbose = TestContext.Properties.Contains("LogVerbose");
            IsLogSuperVerbose = TestContext.Properties.Contains("LogSuperVerbose");

            if (TestContext.Properties.Contains("WaitForDebugger"))
#else
            IsLogVerbose = TestContext.Properties.ContainsKey("LogVerbose");
            IsLogSuperVerbose = TestContext.Properties.ContainsKey("LogSuperVerbose");

            if (TestContext.Properties.ContainsKey("WaitForDebugger"))
#endif
            {
                var processId = Process.GetCurrentProcess().Id;
                while (!Debugger.IsAttached)
                {
                    Log.Comment(string.Format("Waiting for a debugger to attach (processId = {0})...", processId));
                    Thread.Sleep(1000);
                }

                Debugger.Break();
            }

            Application = CreateApplication(type);

            // Initialize relies on TestEnvironment.Application to be set, so we'll call this method
            // outside of the constructor.
#if USING_TAEF
            Application.Initialize(true, TestContext.TestDeploymentDir);
#else
            Application.Initialize(true);
#endif
        }

        public static void ScheduleAppRestartIfNeeded()
        {
#if BUILD_WINDOWS
            // In CatGates, our test app is automatically closed on phone after a certain time elapses
            // if we don't close it ourselves. This leads to test instability, so to ensure we
            // never run into this, we'll restart our application after every test.
            // This increases test runtime, but is necessary to ensure stability.
            // We'll use TestCleanupHelper.TestSetupHelperPendingDisposals == 0 to indicate that
            // we're back on the main page and that therefore the test is complete,
            // unless we're explicitly told it's not.
            if (PlatformConfiguration.IsDevice(DeviceType.Phone))
            {
                Log.Comment("Queueing an app restart to ensure test stability on phone.");
                TestEnvironment.ShouldRestartApplication = true;
            }
#endif
        }

        public static void LogDumpTree(UIObject root = null)
        {
            Log.Comment("============ Dump Tree ============");
            LogDumpTreeWorker("", root ?? Application.CoreWindow);
            Log.Comment("===================================");
        }

        public static void WaitUntilElementLoadedById(string automationId)
        {
            WaitUntilElementLoaded(automationId, ElementKeyType.AutomationId);
        }

        public static void WaitUntilElementLoadedByName(string name)
        {
            WaitUntilElementLoaded(name, ElementKeyType.Name);
        }

        public static void WaitUntilElementLoadedByNameOrId(string nameOrId)
        {
            WaitUntilElementLoaded(nameOrId, ElementKeyType.NameOrAutomationId);
        }

        public static void WaitUntilElementLoadedByClassName(string className)
        {
            WaitUntilElementLoaded(className, ElementKeyType.ClassName);
        }

        private enum ElementKeyType
        {
            AutomationId,
            Name,
            NameOrAutomationId,
            ClassName,
        }

        private static void WaitUntilElementLoaded(string key, ElementKeyType keyType)
        {
            // Wait until the element with the specified element has been loaded before continuing.
            // ElementAddedWaiter doesn't seem to work for this purpose, so this is the next best thing.
            int triesLeft = 50;
            UIObject element = null;

            using (TimeWaiter waiter = new TimeWaiter(TimeSpan.FromMilliseconds(100)))
            {
                while (element == null && triesLeft-- > 0)
                {
                    switch (keyType)
                    {
                        case ElementKeyType.AutomationId:
                            element = TryFindElement.ById(key);
                            break;
                        case ElementKeyType.Name:
                            element = TryFindElement.ByName(key);
                            break;
                        case ElementKeyType.NameOrAutomationId:
                            element = TryFindElement.ByNameOrId(key);
                            break;
                        case ElementKeyType.ClassName:
                            element = TryFindElement.ByClassName(key);
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Invalid element key type: {0}", keyType.ToString()));
                    }

                    waiter.Wait();
                    waiter.Reset();
                }
            }

            if (triesLeft == 0)
            {
                throw new WaiterTimedOutException(string.Format("Could not find '{0}'!", key));
            }
        }

        private static void LogDumpTreeWorker(string indent, UIObject current)
        {
            Log.Comment(indent + current.GetDescription());
            indent += "  ";
            foreach (var uiObject in current.Children)
            {
                LogDumpTreeWorker(indent, uiObject);
            }
        }

        public static void VerifyAreEqualWithRetry(int maxRetries, Func<object> expectedFunc, Func<object> actualFunc, Action retryAction = null)
        {
            if (retryAction == null)
            {
                retryAction = () =>
                {
                    Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
                    ElementCache.Clear(); /* Test is flaky sometimes -- perhaps element cache is stale? Clear it and try again. */
                };
            }

            for (int retry = 0; retry <= maxRetries; retry++)
            {
                object expected = expectedFunc();
                object actual = actualFunc();
                if (Object.Equals(expected, actual) || retry == maxRetries)
                {
                    Log.Comment("Actual retry times: " + retry);
                    Verify.AreEqual(expected, actual);
                    return;
                }
                else
                {
                    retryAction();
                }
            }
        }
    }
}
