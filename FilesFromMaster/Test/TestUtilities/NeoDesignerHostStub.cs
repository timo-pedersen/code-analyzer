using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Screen.ScreenDesign;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor;
using Neo.ApplicationFramework.Tools.Selection;
using Rhino.Mocks;

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
            IScreenRootDesigner screenRootDesigner = MockRepository.GenerateStub<IScreenRootDesigner>();
            NeoDesignerHostStub designerHost = new NeoDesignerHostStub(serviceContainer, screenRootDesigner);

            designerHost.SelectionService = new SelectionService();
            designerHost.ServiceContainer.AddService(typeof(ISelectionService), designerHost.SelectionService);

            designerHost.ScreenEditorWindow = new ScreenEditorTestWindow();
            designerHost.ScreenEditorWindow.Editor.Canvas.ServiceProvider = serviceContainer;
            designerHost.ScreenEditorWindow.Show();

            designerHost.ScreenRootDesigner.Stub(x => x.DesignerHost).Return(designerHost);
            designerHost.ScreenRootDesigner.Stub(x => x.Select(Arg<FrameworkElement>.Is.Anything)).Do(new Action<FrameworkElement>(y => designerHost.SelectionService.SetSelectedComponents(new List<object>() { y })));
            designerHost.ScreenRootDesigner.Stub(x => x.Select(Arg<FrameworkElement>.Is.Anything, Arg<SelectionTypes>.Is.Anything)).Do(new Action<FrameworkElement, SelectionTypes>((y, z) => designerHost.SelectionService.SetSelectedComponents(new List<object>() { y }, z)));
            designerHost.ScreenRootDesigner.Stub(x => x.Select(Arg<IList<FrameworkElement>>.Is.Anything, Arg<SelectionTypes>.Is.Anything)).Do(new Action<IList<FrameworkElement>, SelectionTypes>((y, z) => designerHost.SelectionService.SetSelectedComponents(y.ToList(), z)));
            designerHost.ScreenRootDesigner.Stub(x => x.SelectedElements).Do(new Func<IList<FrameworkElement>>(() => designerHost.SelectionService.GetSelectedComponents().Cast<FrameworkElement>().ToList()));

            IElementChangeService changeService = MockRepository.GenerateStub<IElementChangeService>();
            designerHost.ServiceContainer.AddService(typeof(IElementChangeService), changeService);

            designerHost.ScreenEditor = MockRepository.GenerateStub<IScreenEditor>();
            Adorner adornerStub = MockRepository.GenerateStub<Adorner>(designerHost.ScreenEditorWindow.Editor);
            designerHost.ScreenEditor.Stub(x => x.EditorAdorner).Return(adornerStub);
            designerHost.ScreenEditor.Stub(x => x.EditorCanvas).Return(designerHost.ScreenEditorWindow.Editor.Canvas);
            designerHost.ScreenEditor.Stub(x => x.AddElements(Arg<IList<FrameworkElement>>.Is.Anything)).Do(new Action<IList<FrameworkElement>>(x => designerHost.AddElementsToScreenEditor(x)));
            designerHost.ScreenEditor.Stub(x => x.RemoveElements(Arg<IList<FrameworkElement>>.Is.Anything)).Do(new Action<IList<FrameworkElement>>(x => designerHost.RemoveElementsFromScreenEditor(x)));
            designerHost.ServiceContainer.AddService(typeof(IScreenEditor), designerHost.ScreenEditor);

            designerHost.ScreenDesignerView = new ScreenDesignerView();
            designerHost.ScreenRootDesigner.Stub(x => x.DefaultView).Return(designerHost.ScreenDesignerView);
            designerHost.ScreenRootDesigner.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Do(new Func<string, FrameworkElement>(y => designerHost.ScreenDesignerView.Elements.Where(z => z.Name == y).FirstOrDefault()));
            designerHost.ScreenRootDesigner.Stub(x => x.FindElementsByName(Arg<string>.Is.Anything)).Do(new Func<string, IEnumerable<FrameworkElement>>(y => designerHost.ScreenDesignerView.Elements.Where(z => z.Name == y)));
            designerHost.ScreenRootDesigner.Stub(x => x.SelectOneElement()).Do(new System.Action(() => designerHost.SelectionService.SetSelectedComponents(new List<object>() { designerHost.ScreenDesignerView.Elements.FirstOrDefault() })));

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
                changeService.Raise(z => z.ElementAdded += null, ScreenEditor, new ElementEventArgs(newElement, string.Empty));
            }
        }

        private void RemoveElementsFromScreenEditor(IList<FrameworkElement> elements)
        {
            IElementChangeService changeService = ServiceProvider.GetService(typeof(IElementChangeService)) as IElementChangeService;

            foreach (FrameworkElement element in elements)
            {
                ScreenEditorWindow.Editor.Canvas.Children.Remove(element);
                changeService.Raise(z => z.ElementRemoved += null, ScreenEditor, new ElementEventArgs(element, string.Empty));
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
