using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Screen.ScreenDesign;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor;
using Neo.ApplicationFramework.Tools.Selection;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools
{
    public class NeoDesignerHostStub : INeoDesignerHost, IDisposable, IContainer
    {
        public NeoDesignerHostStub(IServiceContainer serviceContainer, IDesignerBase rootDesigner)
        {
            ServiceContainer = serviceContainer;
            ServiceProvider = serviceContainer;
            RootDesigner = rootDesigner;
        }

        protected IDesignerBase RootDesigner { get; }

        protected IServiceContainer ServiceContainer { get; }

        protected IServiceProvider ServiceProvider { get; }

        protected IComponent RootComponent { get; private set; }

        public ScreenEditorTestWindow ScreenEditorWindow { get; private set; }

        public IScreenRootDesigner ScreenRootDesigner
        {
            get { return RootDesigner as IScreenRootDesigner; }
        }

        public IScreenEditor ScreenEditor { get; private set; }

        public IScreenDesignerView ScreenDesignerView { get; private set; }

        public ISelectionService SelectionService { get; private set; }

        public static NeoDesignerHostStub CreateScreenDesignerHost(IServiceContainer serviceContainer)
        {
            IScreenRootDesigner screenRootDesigner = Substitute.For<IScreenRootDesigner>();
            NeoDesignerHostStub designerHost = new(serviceContainer, screenRootDesigner);

            designerHost.SelectionService = new SelectionService();
            designerHost.ServiceContainer.AddService(typeof(ISelectionService), designerHost.SelectionService);

            designerHost.ScreenEditorWindow = new ScreenEditorTestWindow();
            designerHost.ScreenEditorWindow.Editor.Canvas.ServiceProvider = serviceContainer;
            designerHost.ScreenEditorWindow.Show();

            designerHost.ScreenRootDesigner.DesignerHost.Returns(designerHost);
            designerHost.ScreenRootDesigner.Select(Arg.Do<FrameworkElement>(y =>
                designerHost.SelectionService.SetSelectedComponents(new List<object>() { y })));
            designerHost.ScreenRootDesigner
                .When(x => x.Select(Arg.Any<FrameworkElement>(), Arg.Any<SelectionTypes>()))
                .Do(y => designerHost.SelectionService.SetSelectedComponents(new List<object>() { (FrameworkElement)y[0] }, (SelectionTypes)y[1]));
            designerHost.ScreenRootDesigner
                .When(x => x.Select(Arg.Any<IList<FrameworkElement>>(), Arg.Any<SelectionTypes>()))
                .Do(y => designerHost.SelectionService.SetSelectedComponents(((IList<FrameworkElement>)y[0]).ToList(), (SelectionTypes)y[1]));
            designerHost.ScreenRootDesigner.SelectedElements
                .Returns(x =>
                    designerHost.SelectionService.GetSelectedComponents().Cast<FrameworkElement>().ToList());

            IElementChangeService changeService = Substitute.For<IElementChangeService>();
            designerHost.ServiceContainer.AddService(typeof(IElementChangeService), changeService);

            designerHost.ScreenEditor = Substitute.For<IScreenEditor>();
            Adorner adornerStub = Substitute.For<Adorner>(designerHost.ScreenEditorWindow.Editor);
            
            designerHost.ScreenEditor.EditorAdorner.Returns(adornerStub);
            designerHost.ScreenEditor.EditorCanvas.Returns(designerHost.ScreenEditorWindow.Editor.Canvas);

            designerHost.ScreenEditor
                .When(x => x.AddElements(Arg.Any<IList<FrameworkElement>>()))
                .Do(y => designerHost.AddElementsToScreenEditor((IList<FrameworkElement>)y[0]));

            designerHost.ScreenEditor
                .When(x => x.RemoveElements(Arg.Any<IList<FrameworkElement>>()))
                .Do(y => designerHost.RemoveElementsFromScreenEditor((IList<FrameworkElement>)y[0]));

            designerHost.ServiceContainer.AddService(typeof(IScreenEditor), designerHost.ScreenEditor);

            designerHost.ScreenDesignerView = new ScreenDesignerView();
            designerHost.ScreenRootDesigner.DefaultView.Returns(designerHost.ScreenDesignerView);

            designerHost.ScreenRootDesigner.FindElementByName(Arg.Any<string>())
                .ReturnsForAnyArgs(x =>
                {
                    return designerHost.ScreenDesignerView.Elements.FirstOrDefault(z => z.Name == (string)x[0]);
                });

            designerHost.ScreenRootDesigner.FindElementsByName(Arg.Any<string>())
                .ReturnsForAnyArgs(x =>
                {
                    return designerHost.ScreenDesignerView.Elements.Where(z => z.Name == (string)x[0]);
                });

            designerHost.ScreenRootDesigner
                .When(x => x.SelectOneElement())
                .Do(y => designerHost.SelectionService.SetSelectedComponents(
                    new List<object>()
                    {
                        designerHost.ScreenDesignerView.Elements.FirstOrDefault()
                    }));

            ((ScreenDesignerView)designerHost.ScreenDesignerView).ScreenEditor = designerHost.ScreenEditor;
            designerHost.ScreenDesignerView.Designer = designerHost.ScreenRootDesigner;

            return designerHost;
        }

        private void AddElementsToScreenEditor(IList<FrameworkElement> newElements)
        {
            IElementChangeService changeService = ServiceProvider.GetService(typeof(IElementChangeService)) as IElementChangeService;

            foreach (FrameworkElement newElement in newElements)
            {
                ScreenEditorWindow.Editor.Canvas.Children.Add(newElement);
                changeService.ElementAdded += Raise.EventWith(ScreenEditor, new ElementEventArgs(newElement, string.Empty));
            }
        }

        private void RemoveElementsFromScreenEditor(IList<FrameworkElement> elements)
        {
            IElementChangeService changeService = ServiceProvider.GetService(typeof(IElementChangeService)) as IElementChangeService;

            foreach (FrameworkElement element in elements)
            {
                ScreenEditorWindow.Editor.Canvas.Children.Remove(element);
                changeService.ElementRemoved += Raise.EventWith(ScreenEditor, new ElementEventArgs(element, string.Empty));
            }

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (ScreenEditorWindow != null)
                ScreenEditorWindow.Close();

        }

        #region INeoDesignerHost Members

        void INeoDesignerHost.Deactivate()
        {
            throw new NotImplementedException();
        }

        void INeoDesignerHost.SetDirty()
        {
            throw new NotImplementedException();
        }

        bool INeoDesignerHost.IsDisposed
        {
            get { throw new NotImplementedException(); }
        }

        IRootDesigner INeoDesignerHost.RootDesigner
        {
            get { return RootDesigner; }
        }

        T INeoDesignerHost.GetService<T>()
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }

        bool INeoDesignerHost.IsViewLoaded
        {
            get { throw new NotImplementedException(); }
        }

        bool INeoDesignerHost.LoadView()
        {
            throw new NotImplementedException();
        }

        event EventHandler INeoDesignerHost.LoadViewComplete
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler INeoDesignerHost.DesignerSaved
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler INeoDesignerHost.SavingDesigner
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler INeoDesignerHost.DesignerCreated
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion

        #region IDesignerHost Members

        void IDesignerHost.Activate()
        {
            throw new NotImplementedException();
        }

        event EventHandler System.ComponentModel.Design.IDesignerHost.Activated
        {
            add { ; }
            remove { ; }
        }

        IContainer IDesignerHost.Container
        {
            get { throw new NotImplementedException(); }
        }

        IComponent IDesignerHost.CreateComponent(Type componentClass, string name)
        {
            throw new NotImplementedException();
        }

        IComponent IDesignerHost.CreateComponent(Type componentClass)
        {
            throw new NotImplementedException();
        }

        DesignerTransaction IDesignerHost.CreateTransaction(string description)
        {
            throw new NotImplementedException();
        }

        DesignerTransaction IDesignerHost.CreateTransaction()
        {
            throw new NotImplementedException();
        }

        event EventHandler System.ComponentModel.Design.IDesignerHost.Deactivated
        {
            add { ; }
            remove { ;}
        }

        void IDesignerHost.DestroyComponent(IComponent component)
        {
            throw new NotImplementedException();
        }

        private IDesigner GetDesigner(IComponent component)
        {
            if (component == RootComponent)
                return RootDesigner;

            return null;
        }
        IDesigner IDesignerHost.GetDesigner(IComponent component)
        {
            return GetDesigner(component);
        }

        Type IDesignerHost.GetType(string typeName)
        {
            throw new NotImplementedException();
        }

        bool IDesignerHost.InTransaction
        {
            get { throw new NotImplementedException(); }
        }

        event EventHandler System.ComponentModel.Design.IDesignerHost.LoadComplete
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        bool IDesignerHost.Loading
        {
            get { throw new NotImplementedException(); }
        }

        IComponent IDesignerHost.RootComponent
        {
            get { return RootComponent; }
        }

        string IDesignerHost.RootComponentClassName
        {
            get { throw new NotImplementedException(); }
        }

        event DesignerTransactionCloseEventHandler System.ComponentModel.Design.IDesignerHost.TransactionClosed
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event DesignerTransactionCloseEventHandler System.ComponentModel.Design.IDesignerHost.TransactionClosing
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        string IDesignerHost.TransactionDescription
        {
            get { throw new NotImplementedException(); }
        }

        event EventHandler System.ComponentModel.Design.IDesignerHost.TransactionOpened
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler System.ComponentModel.Design.IDesignerHost.TransactionOpening
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion

        #region IServiceContainer Members

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            ServiceContainer.AddService(serviceType, callback, promote);
        }

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            ServiceContainer.AddService(serviceType, callback);
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
        {
            ServiceContainer.AddService(serviceType, serviceInstance, promote);
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance)
        {
            ServiceContainer.AddService(serviceType, serviceInstance);
        }

        void IServiceContainer.RemoveService(Type serviceType, bool promote)
        {
            ServiceContainer.RemoveService(serviceType, promote);
        }

        void IServiceContainer.RemoveService(Type serviceType)
        {
            ServiceContainer.RemoveService(serviceType);
        }

        #endregion

        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Dispose();
        }

        #endregion

        #region IContainer Members

        void IContainer.Add(IComponent component, string name)
        {
            throw new NotImplementedException();
        }

        void IContainer.Add(IComponent component)
        {
            throw new NotImplementedException();
        }

        ComponentCollection IContainer.Components
        {
            get { throw new NotImplementedException(); }
        }

        void IContainer.Remove(IComponent component)
        {
            throw new NotImplementedException();
        }
        
        #endregion 
    }
}
