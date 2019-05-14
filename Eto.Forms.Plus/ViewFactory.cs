using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Eto.Forms.Plus
{
	public class ViewFactory
	{
		private Func<Type, object> _factory; // This is assigned by the ctor

		/// <summary>
		/// Gets or sets the delegate used to retrieve an instance of a view
		/// </summary>
		public Func<Type, object> Factory
		{
			get { return this._factory; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this._factory = value;
			}
		}

		private List<Assembly> _viewAssemblies;

		/// <summary>
		/// Gets or sets the assemblies which are used for IoC container auto-binding and searching for Views.
		/// </summary>
		public List<Assembly> ViewAssemblies
		{
			get { return _viewAssemblies; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				_viewAssemblies = value;
			}
		}

		private Dictionary<string, string> _namespaceTransformations = new Dictionary<string, string>();

		/// <summary>
		/// Gets or sets a set of transformations to be applied to the ViewModel's namespace: string to find -> string to replace it with
		/// </summary>
		public Dictionary<string, string> NamespaceTransformations
		{
			get { return this._namespaceTransformations; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this._namespaceTransformations = value;
			}
		}

		private string _viewNameSuffix = "View";

		/// <summary>
		/// Gets or sets the suffix replacing 'ViewModel' (see <see cref="ViewModelNameSuffix"/>). Defaults to 'View'
		/// </summary>
		public string ViewNameSuffix
		{
			get { return this._viewNameSuffix; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this._viewNameSuffix = value;
			}
		}

		private string _viewModelNameSuffix = "ViewModel";

		/// <summary>
		/// Gets or sets the suffix of ViewModel names, defaults to 'ViewModel'. This will be replaced by <see cref="ViewNameSuffix"/>
		/// </summary>
		public string ViewModelNameSuffix
		{
			get { return this._viewModelNameSuffix; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this._viewModelNameSuffix = value;
			}
		}

		public ViewFactory()
		{
			_viewAssemblies = new List<Assembly> { GetType().Assembly };
		}

		public Window GetAndBind<TViewModel>(TViewModel viewModel)
		{
			var view = CreateViewForModel(viewModel);
			BindViewToModel(view, viewModel);

			if (viewModel is ViewModelBase viewModelBase)
			{
				viewModelBase.View = view;
				view.Load += (s, e) => viewModelBase.OnViewLoaded();
				view.Shown += (s, e) => viewModelBase.OnViewShown();
                view.UnLoad += (s, e) => viewModelBase.OnViewUnloaded();

                if (view is Window window)
                {
                    window.Closed += (s, e) => viewModelBase.OnViewClosed();
                }
			}

			return view as Window;
		}

		/// <summary>
		/// Given the expected name for a view, locate its type (or return null if a suitable type couldn't be found)
		/// </summary>
		/// <param name="viewName">View name to locate the type for</param>
		/// <param name="extraAssemblies">Extra assemblies to search through</param>
		/// <returns>Type for that view name</returns>
		protected virtual Type ViewTypeForViewName(string viewName, IEnumerable<Assembly> extraAssemblies)
		{
			return ViewAssemblies.Concat(extraAssemblies).Select(x => x.GetType(viewName)).FirstOrDefault(x => x != null);
		}

		/// <summary>
		/// Given the full name of a ViewModel type, determine the corresponding View type nasme
		/// </summary>
		/// <remarks>
		/// This is used internally by LocateViewForModel. If you override LocateViewForModel, you
		/// can simply ignore this method.
		/// </remarks>
		/// <param name="modelTypeName">ViewModel type name to get the View type name for</param>
		/// <returns>View type name</returns>
		protected virtual string ViewTypeNameForModelTypeName(string modelTypeName)
		{
			string transformed = modelTypeName;

			foreach (var transformation in this.NamespaceTransformations)
			{
				if (transformed.StartsWith(transformation.Key + "."))
				{
					transformed = transformation.Value + transformed.Substring(transformation.Key.Length);
					break;
				}
			}

			transformed = Regex.Replace(transformed,
				String.Format(@"(?<=.){0}(?=s?\.)|{0}$", Regex.Escape(this.ViewModelNameSuffix)),
				Regex.Escape(this.ViewNameSuffix));

			return transformed;
		}

		/// <summary>
		/// Given the type of a model, locate the type of its View (or throw an exception)
		/// </summary>
		/// <param name="modelType">Model to find the view for</param>
		/// <returns>Type of the ViewModel's View</returns>
		protected virtual Type LocateViewForModel(Type modelType)
		{
			var modelName = modelType.FullName;
			var viewName = ViewTypeNameForModelTypeName(modelName);
			if (modelName == viewName)
				throw new Exception(String.Format("Unable to transform ViewModel name {0} into a suitable View name", modelName));

			// Also include the ViewModel's assembly, to be helpful
			var viewType = this.ViewTypeForViewName(viewName, new[] { modelType.Assembly });
			if (viewType == null)
			{
				var e = new Exception(String.Format("Unable to find a View with type {0}", viewName));
				//logger.Error(e);
				throw e;
			}
			else
			{
				//logger.Info("Searching for a View with name {0}, and found {1}", viewName, viewType);
			}

			return viewType;
		}

		/// <summary>
		/// Given a ViewModel instance, locate its View type (using LocateViewForModel), and instantiates it
		/// </summary>
		/// <param name="model">ViewModel to locate and instantiate the View for</param>
		/// <returns>Instantiated and setup view</returns>
		protected virtual Control CreateViewForModel(object model)
		{
			var viewType = this.LocateViewForModel(model.GetType());

			if (viewType.IsAbstract || !typeof(Control).IsAssignableFrom(viewType))
			{
				var e = new Exception(String.Format("Found type for view: {0}, but it wasn't a class derived from UIElement", viewType.Name));
				//logger.Error(e);
				throw e;
			}

			var view = (Control)Factory(viewType);

			InitializeView(view, viewType);

			return view;
		}

		/// <summary>
		/// Given a view, take steps to initialize it (for example calling InitializeComponent)
		/// </summary>
		/// <param name="view">View to initialize</param>
		/// <param name="viewType">Type of view, passed for efficiency reasons</param>
		public virtual void InitializeView(Control view, Type viewType)
		{
			// If it doesn't have a code-behind, this won't be called
			// We have to use this reflection here, since the InitializeComponent is a method on the View, not on any of its base classes
			//var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
			//if (initializer != null)
			//	initializer.Invoke(view, null);

			//  todo: Consider putting Xaml.Load(this) call here
		}

		/// <summary>
		/// Given an instance of a ViewModel and an instance of its View, bind the two together
		/// </summary>
		/// <param name="view">View to bind to the ViewModel</param>
		/// <param name="viewModel">ViewModel to bind the View to</param>
		public virtual void BindViewToModel(Control view, object viewModel)
		{
			//logger.Info("Setting {0}'s ActionTarget to {1}", view, viewModel);
			//View.SetActionTarget(view, viewModel);

			var bindableWidget = view as BindableWidget;
			if (bindableWidget != null)
			{
				//logger.Info("Setting {0}'s DataContext to {1}", view, viewModel);
				bindableWidget.DataContext = viewModel;
			}

			//var viewModelAsViewAware = viewModel as IViewAware;
			//if (viewModelAsViewAware != null)
			//{
			//	logger.Info("Setting {0}'s View to {1}", viewModel, view);
			//	viewModelAsViewAware.AttachView(view);
			//}
		}
	}
}
