using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using CodeInSanityTest.SolutionParser;
using CodeInSanityTest.Utilities;
using Core.Api.DI.PlatformFactory;
using Core.Api.Service;
using Core.Controls.Api.Bindings;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.OpcClient.Bindings;
using Neo.ApplicationFramework.Interfaces;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.TestUtilities
{
    public static class TestHelper
    {
        /// <summary>
        /// Use a WindowThreadHelper in NeoApplication that does not do anything asynchronous
        /// , and does not marshal anything between threads, and is therefore suited for test purposes.
        /// </summary>
        public static bool UseTestWindowThreadHelper
        {
            get { return (NeoApplication.WindowThreadingHelper is TestThreadHelper); }
            set
            {
                if (value && !(NeoApplication.WindowThreadingHelper is TestThreadHelper))
                {
                    NeoApplication.WindowThreadingHelper = new TestThreadHelper();
                }
                if (!value)
                {
                    NeoApplication.WindowThreadingHelper = null;
                }
            }
        }

        /// <summary>
        /// Use this method to call a static method that is inasccessible due to its protection level.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object RunStaticMethod(Type type, string methodName, params object[] parameters)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            return RunMethod(type, methodName, null, bindingFlags, parameters);
        }

        public static object RunInstanceMethod(Type type, string methodName, object instance, params object[] parameters)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return RunMethod(type, methodName, instance, bindingFlags, parameters);
        }

        private static object RunMethod(Type type, string methodName, object instance, BindingFlags bindingFlags, params object[] parameters)
        {
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags, null, GetTypesArray(parameters), null);

            if (methodInfo == null)
            {
                throw new ArgumentException("There is no method '" + methodName + "' for type '" + type + "'.");
            }

            object returnedObject = methodInfo.Invoke(instance, parameters);
            return returnedObject;
        }

        private static Type[] GetTypesArray(object[] parameters)
        {
            Type[] types = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].GetType();
            }

            return types;
        }

        public static object GetInstanceProperty(Type type, object instance, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo == null)
                return null; // throw

            return propertyInfo.GetValue(instance, null);
        }

        public static void SetInstanceProperty(Type type, object instance, string propertyName, object value)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo == null)
                return; // throw

            propertyInfo.SetValue(instance, value, null);
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
                return null; // throw

            return fieldInfo.GetValue(instance);
        }

        public static void SetInstanceField(Type type, object instance, string fieldName, object value)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
                return; // throw

            fieldInfo.SetValue(instance, value);
        }

        public static void SetSingleInstanceField(Type type, object instance, Type fieldType, object value)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var field = fields.Single(item => item.FieldType == fieldType);
            field.SetValue(instance, value);
        }

        public static void SetStaticField(Type type, string fieldName, object value)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
                return; // throw

            fieldInfo.SetValue(null, value);
        }

        public static void AddService(Type serviceType, object serviceInstance)
        {
            ServiceContainerCF.Instance.AddService(serviceType, serviceInstance);
        }

        public static void AddService<TService>(TService serviceInstance)
        {
            AddService(typeof(TService), serviceInstance);
        }

        public static TService AddServiceStub<TService>() where TService : class
        {
            var stub = MockRepository.GenerateStub<TService>();

            AddService(stub);

            return stub;
        }

        public static T RemoveService<T>() where T : class
        {
            T service = ServiceContainerCF.Instance.GetService(typeof(T)) as T;
            ServiceContainerCF.Instance.RemoveService(typeof(T));

            return service;
        }

        public static void ClearServices()
        {
            ServiceContainerCF.Instance.Clear();
        }

        public static void SetupServicePlatformFactory<TRequestedType>(TRequestedType typeImplementation) where TRequestedType : class
        {
            IPlatformFactoryService factory = ServiceContainerCF.GetServiceSafe<IPlatformFactoryService>();
            if (factory == null)
                factory = AddServiceStub<IPlatformFactoryService>();

            factory.Stub(x => x.Create<TRequestedType>()).Return(typeImplementation);
        }


        public static IProjectManager AddServiceProjectManager(MockRepository mockRepository)
        {
            IProjectManager projectManager = CreateAndAddServiceMock<IProjectManager>(mockRepository);
            IProject project = MockRepository.GenerateStub<IProject>();

            SetupResult.For(projectManager.Project).Return(project);

            return projectManager;
        }

        public static TService CreateAndAddServiceMock<TService>(MockRepository mockRepository) where TService : class
        {
            TService serviceInstance = mockRepository.DynamicMock<TService>();
            AddService(typeof(TService), serviceInstance);
            return serviceInstance;
        }

        public static TService CreateAndAddServiceMock<TService>() where TService : class
        {
            TService serviceInstance = MockRepository.GenerateMock<TService>();
            AddService(typeof(TService), serviceInstance);
            return serviceInstance;
        }

        public static TService CreateAndAddServiceStub<TService>() where TService : class
        {
            TService serviceInstance = MockRepository.GenerateStub<TService>();
            AddService(typeof(TService), serviceInstance);
            return serviceInstance;
        }

        public static Version GetPreviousVersion(Version version)
        {
            if (version.Build > 0)
            {
                return new Version(version.Major, version.Minor, version.Build - 1, 0);
            }
            else if (version.Minor > 0)
            {
                return new Version(version.Major, version.Minor - 1, 0, 0);
            }

            return new Version(1, 0, 0, 0);
        }

        /// <summary>
        /// Gets the directory path of the calling assembly.
        /// </summary>
        public static string CurrentDirectory
        {
            get
            {
                string assemblyFilePath = Assembly.GetCallingAssembly().Location;
                return Path.GetDirectoryName(assemblyFilePath);
            }
        }

        /// <summary>
        /// Gets the current version of the designer.
        /// </summary>
        public static Version CurrentDesignerVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static IEnumerable<Assembly> LoadAssemblies(string solutionName, string buildConfig)
        {
            var resultList = new List<Assembly>();

            Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            IEnumerable<SolutionProject> projects = SolutionHelper.GetSolutionProjects(SolutionDirectory, solutionName);

            foreach (SolutionProject solutionProject in projects)
            {
                var assemblyName = new AssemblyName(solutionProject.ProjectName);

                if (currentAssemblies.Any(x => x.FullName.Contains(solutionProject.ProjectName)))
                    continue;

                try
                {
                    string assemblyFilePath = Path.Combine(SolutionDirectory, buildConfig, assemblyName.Name + ".dll");
                    Assembly assembly = Assembly.LoadFrom(assemblyFilePath);
                    resultList.Add(assembly);
                }
                catch (Exception)
                { }
            }
            resultList.AddRange(currentAssemblies);
            return resultList;
        }

        //Sort of expects to be executed in something like "iX\Tools\ToolsTestx86\bin\Debug\"
        public static string SolutionDirectory => PathTools.GetParentDir(AppDomain.CurrentDomain.BaseDirectory, 4).FullName;

        public static class Bindings
        {
            public static class Wpf
            {
                public static void ClearProviders()
                {
                    DependencyObjectPropertyBinder.ClearBindingSourceDescriptionProviders();
                }

                public static void RegisterSimpleDataItemBindingSourceProvider()
                {
                    var bindingService = AddServiceStub<IBindingService>();
                    bindingService
                        .Stub(mock => mock.IsSupporting(default(Binding))).IgnoreArguments()
                        .Do(new Func<Binding, bool>(CanHandleBinding));

                    bindingService
                        .Stub(mock => mock.ProvideBindingSourceDescription(default(Binding))).IgnoreArguments()
                        .Do(new Func<Binding, BindingSourceDescription>(
                            binding => CanHandleBinding(binding) ? GetBindingSourceDescription(binding) : null));
                }

                private static bool CanHandleBinding(Binding binding)
                {
                    if (binding.Path.Path.IndexOf("[") < 0)
                        return false;

                    if (binding.Path.Path.IndexOf("]") < 0)
                        return false;

                    return true;
                }

                private static BindingSourceDescription GetBindingSourceDescription(Binding binding)
                {
                    string path = binding.Path.Path;
                    int startIndex = path.IndexOf("[") + 1;
                    int endIndex = path.IndexOf(']');
                    string tagName = path.Substring(startIndex, endIndex - startIndex);

                    return DataItemBindingSourceDescription.CreateFromFullName(tagName);
                }
            }
        }
    }
}
